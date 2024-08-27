using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using PluginAPI.Core;
using UnityEngine.Rendering.LookDev;
using UnityEngine.UIElements;

namespace HintServiceMeow.Core.Utilities
{
    internal static class HintParser
    {
        private static readonly StringBuilder MessageBuilder = new StringBuilder(ushort.MaxValue); //Used to build display text

        private static readonly StringBuilder RichTextBuilder = new StringBuilder(500); //Used to build rich text from Hint

        private static readonly List<Hint> HintList = new List<Hint>();

        private static readonly Dictionary<Guid, ValueTuple<float, float>> _dynamicHintPositionCache = new Dictionary<Guid, ValueTuple<float, float>>();

        public static string GetMessage(HashSet<AbstractHint> rawHintList, PlayerDisplay pd)
        {
            HintList.Clear();

            foreach (var item in rawHintList)
            {
                if (item.Hide || string.IsNullOrEmpty(item.Content.GetText()))
                    continue;

                if (item is Hint hint)
                    HintList.Add(hint);
                else if (item is DynamicHint dynamicHint)
                    HintList.Add(ConvertDynamicHint(dynamicHint, HintList));
            }

            return GetText(HintList);
        }

        public static Hint ConvertDynamicHint(DynamicHint dynamicHint, List<Hint> hintList)
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

            if (_dynamicHintPositionCache.TryGetValue(dynamicHint.Guid, out var cachedPosition))
            {
                //If cached position is not intersected with any hint, use it
                if(!HasIntersection(cachedPosition.Item1, cachedPosition.Item2))
                {
                    return new Hint(dynamicHint, cachedPosition.Item1, cachedPosition.Item2);
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
                    _dynamicHintPositionCache[dynamicHint.Guid] = ValueTuple.Create(x, y);
                    return new Hint(dynamicHint, x, y);
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
                return new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY);

            return null;
        }

        private static string GetText(IEnumerable<Hint> hintList)
        {
            MessageBuilder.Clear();

            MessageBuilder.AppendLine("<line-height=0><voffset=9999>P</voffset>");//Place Holder

            foreach (Hint hint in hintList)
            {
                var text = ToRichText(hint);
                if (!string.IsNullOrEmpty(text))
                    MessageBuilder.AppendLine(text);
            }

            MessageBuilder.AppendLine("<line-height=0><voffset=-9999>P</voffset>");//Place Holder

            var result = MessageBuilder.ToString();
            MessageBuilder.Clear();

            return result;
        }

        private static string ToRichText(Hint hint)
        {
            //Remove Illegal Tags
            string rawText = hint.Content.GetText() ?? string.Empty;

            string text = rawText;

            if(hint is CompatAdapterHint)
            {
                text = Regex
                    .Replace(
                        text,
                        @"<line-height=\d+>|<voffset=[+-]?\d+>|<pos=[+-]?\d+>|<align=(left|center|right)>|</voffset>|</align>|{|}",
                        string.Empty,
                        RegexOptions.IgnoreCase | RegexOptions.Compiled
                    );

                text += "</size></b></i></color>"; //Add closing tag for size to make sure it will not affect other hints
            }
            else
            {
                text = Regex
                    .Replace(
                        text,
                        @"<line-height=\d+>|<voffset=[+-]?\d+>|<pos=[+-]?\d+>|<align=(left|center|right)>|<size=\d+>|</voffset>|</align>|</size>{|}",
                        string.Empty,
                        RegexOptions.IgnoreCase | RegexOptions.Compiled
                    );
            }

            if (string.IsNullOrEmpty(text))
                return null;

            //Building rich text by lines
            var lineList = text.Split('\n');
            float yOffset = 0;

            RichTextBuilder.Clear();

            foreach (var line in lineList)
            {
                float xCoordinate = hint.XCoordinate;
                float yCoordinate = CoordinateTools.GetVOffset(hint) - yOffset;

                if (xCoordinate != 0) RichTextBuilder.AppendFormat("<pos={0:0.#}>", xCoordinate);
                if (hint.Alignment != HintAlignment.Center) RichTextBuilder.AppendFormat("<align={0}>", hint.Alignment);
                RichTextBuilder.Append("<line-height=0>");
                if (yCoordinate != 0) RichTextBuilder.AppendFormat("<voffset={0:0.#}>", yCoordinate);
                RichTextBuilder.AppendFormat("<size={0}>", hint.FontSize);

                RichTextBuilder.Append(line);

                RichTextBuilder.Append("</size>");
                if (yCoordinate != 0) RichTextBuilder.Append("</voffset>");
                if (hint.Alignment != HintAlignment.Center) RichTextBuilder.Append("</align>");
                RichTextBuilder.AppendLine();

                yOffset += hint.FontSize + hint.LineHeight;
            }

            var result = RichTextBuilder.ToString();

            RichTextBuilder.Clear();

            return result;
        }
    }
}

