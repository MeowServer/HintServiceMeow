using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using PluginAPI.Core;
using UnityEngine.UIElements;

namespace HintServiceMeow.Core.Utilities
{
    internal static class HintParser
    {
        private static Dictionary<DynamicHint, ValueTuple<float, float>> _dynamicHintPositionCache = new Dictionary<DynamicHint, ValueTuple<float, float>>();

        public static string GetMessage(HashSet<AbstractHint> rawHintList, PlayerDisplay pd)
        {
            List<Hint> hintList = rawHintList
                .Where( x => !x.Hide && !string.IsNullOrEmpty(x.Content.GetText()) )
                .OfType<Hint>()
                .ToList();

            List<DynamicHint> dynamicHintList = rawHintList
                .Where( x => !x.Hide && !string.IsNullOrEmpty(x.Content.GetText()) )
                .OfType<DynamicHint>()
                .ToList();

            foreach (var dynamicHint in dynamicHintList)
            {
                InsertDynamicHint(dynamicHint, hintList);
            }

            return GetText(hintList);
        }

        public static void InsertDynamicHint(DynamicHint dynamicHint, List<Hint> hintList)
        {
            bool HasIntersection(float x, float y)
            {
                return hintList.Any(hint =>
                {
                    var xCoordinate = CoordinateTools.GetXCoordinateWithAlignment(hint);
                    var yCoordinate = CoordinateTools.GetActualYCoordinate(hint, hint.YCoordinateAlign);

                    float leftA = xCoordinate - CoordinateTools.GetTextWidth(hint) / 2;
                    float rightA = xCoordinate + CoordinateTools.GetTextWidth(hint) / 2;
                    float topA = yCoordinate - CoordinateTools.GetTextHeight(hint);
                    float bottomA = yCoordinate;

                    float leftB = x - CoordinateTools.GetTextWidth(dynamicHint) / 2 - 40;// 40 units of horizontal margin and 5 unity of vertical margin
                    float rightB = x + CoordinateTools.GetTextWidth(dynamicHint) / 2 + 40;
                    float topB = y - CoordinateTools.GetTextHeight(dynamicHint) - 5;
                    float bottomB = y + 5;

                    return !(leftA >= rightB || leftB >= rightA || topA >= bottomB || topB >= bottomA);
                });
            }

            if (_dynamicHintPositionCache.TryGetValue(dynamicHint, out var cachedPosition))
            {
                //If cached position is not intersected with any hint, use it
                if(!HasIntersection(cachedPosition.Item1, cachedPosition.Item2))
                {
                    hintList.Add(new Hint(dynamicHint, cachedPosition.Item1, cachedPosition.Item2));
                    return;
                }
            }

            //If there's no cached position or cached position is not usable, then find new position
            var queue = new Queue<ValueTuple<float, float>>();
            var visited = new HashSet<ValueTuple<float, float>>();

            queue.Enqueue(ValueTuple.Create(dynamicHint.TargetX, dynamicHint.TargetY));

            while (queue.Count > 0)
            {
                //Represent bottom center coordinate of the hint
                var (x, y) = queue.Dequeue();

                if (visited.Contains(ValueTuple.Create(x, y)))
                    continue;

                visited.Add(ValueTuple.Create(x, y));

                if (!HasIntersection(x, y))
                {
                    //Found a position that does not overlap with any hint. Add into cache
                    _dynamicHintPositionCache[dynamicHint] = ValueTuple.Create(x, y);
                    hintList.Add(new Hint(dynamicHint, x, y));
                    return;
                }

                //For performance, dynamic hint will search 20 by 20 horizontally and 70 by 70 vertically
                if(y + 20 < dynamicHint.BottomBoundary)
                    queue.Enqueue(ValueTuple.Create(x, y + 20));
                if(y - 20 > dynamicHint.TopBoundary)
                    queue.Enqueue(ValueTuple.Create(x, y - 20));
                if (x + 70 < dynamicHint.RightBoundary)
                    queue.Enqueue(ValueTuple.Create(x + 70, y));
                if (x - 70 > dynamicHint.LeftBoundary)
                    queue.Enqueue(ValueTuple.Create(x - 70, y));
            }

            if (dynamicHint.Strategy == DynamicHintStrategy.StayInPosition)
                hintList.Add(new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY));
        }

        private static string GetText(List<Hint> hintList)
        {
            //Sort by Y coordinate
            hintList.Sort((x, y) => x.YCoordinate.CompareTo(y.YCoordinate));

            //Convert all hints to rich text
            List<string> textList = hintList
                .Select(hint => ToRichText(hint))
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList();

            //Place Holder
            textList.Insert(0, "<line-height=0><voffset=10000><size=40>PH</voffset></size>");
            textList.Add("<line-height=0><voffset=-10000><size=40>PH</voffset></size>");

            return string.Join("\n", textList);
        }

        private static string ToRichText(Hint hint)
        {
            //Remove Illegal Tags
            string rawText = hint.Content.GetText() ?? string.Empty;

            string text = Regex
                .Replace(rawText, @"<line-height=(\d+(\.\d+)?)>|<voffset=(-?\d+(\.\d+)?)>|<pos=(\d+(\.\d+)?)>|<align=(left|center|right)>|<fontsize=(\d+(\.\d+)?)>", string.Empty)
                .Replace("</voffset>", string.Empty)
                .Replace("</align>", string.Empty);

            if (string.IsNullOrEmpty(text))
                return null;

            //Building rich text by lines
            StringBuilder sb = new StringBuilder();
            var lineList = text.Split('\n');
            float yOffset = 0;

            foreach (var line in lineList)
            {
                float xCoordinate = hint.XCoordinate;
                float yCoordinate = CoordinateTools.GetVOffset(hint) - yOffset;

                if (xCoordinate != 0) sb.Append($"<pos={xCoordinate:0.#}>");
                if (hint.Alignment != HintAlignment.Center) sb.Append($"<align={hint.Alignment}>");
                sb.Append("<Line-height=0>");
                if (yCoordinate != 0) sb.Append($"<voffset={yCoordinate:0.#}>");
                sb.Append($"<size={hint.FontSize}>");

                sb.Append(line);

                sb.Append("</size>");
                if (yCoordinate != 0) sb.Append("</voffset>");
                if (hint.Alignment != HintAlignment.Center) sb.Append("</align>");

                sb.Append('\n');

                yOffset += hint.FontSize;
            }

            return sb.ToString();
        }
    }
}

