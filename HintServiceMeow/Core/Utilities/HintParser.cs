using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using PluginAPI.Core;
using UnityEngine.Rendering.LookDev;
using UnityEngine.UIElements;

namespace HintServiceMeow.Core.Utilities
{
    internal class HintParser
    {
        private readonly object _lock = new object();

        private readonly StringBuilder _messageBuilder = new StringBuilder(ushort.MaxValue); //Used to build display text

        private readonly StringBuilder _richTextBuilder = new StringBuilder(500); //Used to build rich text from Hint

        private readonly List<List<Hint>> _hintList = new List<List<Hint>>();

        private readonly Dictionary<Guid, ValueTuple<float, float>> _dynamicHintPositionCache = new Dictionary<Guid, ValueTuple<float, float>>();

        public string GetMessage(HintCollection collection)
        {
            lock (_lock)
            {
                _hintList.Clear();

                foreach (var group in collection.AllGroups)
                {
                    List<Hint> orderedList = new List<Hint>();

                    //Convert to Hint
                    foreach (var item in group)
                    {
                        if (item.Hide || string.IsNullOrEmpty(item.Content.GetText()))
                            continue;

                        if (item is Hint hint)
                            orderedList.Add(hint);
                        else if (item is DynamicHint dynamicHint)
                            orderedList.Add(ConvertDynamicHint(dynamicHint, orderedList));
                    }

                    //Sort by y coordinate and priority
                    orderedList.Sort((x, y) => CoordinateTools.GetActualYCoordinate(x, x.YCoordinateAlign).CompareTo(CoordinateTools.GetActualYCoordinate(y, y.YCoordinateAlign)));

                    _hintList.Add(orderedList);
                }

                _messageBuilder.Clear();
                _messageBuilder.AppendLine("<line-height=0><voffset=9999>P</voffset>");//Place Holder

                foreach (List<Hint> hintList in _hintList)
                {
                    foreach (Hint hint in hintList)
                    {
                        var text = ToRichText(hint);
                        if (!string.IsNullOrEmpty(text))
                            _messageBuilder.Append(text);//ToRichText already added \n at the end
                    }

                    _messageBuilder.AppendLine("</size></b></i>"); //Make sure one group will not affect another group
                }

                _messageBuilder.AppendLine("<line-height=0><voffset=-9999>P</voffset>");//Place Holder

                var result = _messageBuilder.ToString();
                _messageBuilder.Clear();

                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{DateTime.Now.Ticks}.txt");
                File.WriteAllText(path, result);

                return result;
            }
        }

        public Hint ConvertDynamicHint(DynamicHint dynamicHint, List<Hint> hintList)
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

        private string ToRichText(Hint hint)
        {
            //Remove Illegal Tags
            string rawText = hint.Content.GetText() ?? string.Empty;

            string text = rawText;

            if(hint is CompatAdapterHint)
            {
                text = Regex
                    .Replace(
                        text,
                        @"<line-height=[^>]*>|<voffset=[^>]*>|<pos=[^>]*>|<align=[^>]*>|</voffset>|</align>|{|}",
                        string.Empty,
                        RegexOptions.IgnoreCase | RegexOptions.Compiled
                    );
            }
            else
            {
                text = Regex
                    .Replace(
                        text,
                        @"<line-height=[^>]*>|<voffset=[^>]*>|<pos=[^>]*>|<align=[^>]*>|<size=\d+>|</voffset>|</align>|</size>{|}",
                        string.Empty,
                        RegexOptions.IgnoreCase | RegexOptions.Compiled
                    );
            }

            if (string.IsNullOrEmpty(text))
                return null;

            //Building rich text by lines
            var lineList = text.Split('\n');
            float yOffset = 0;

            _richTextBuilder.Clear();

            foreach (var line in lineList)
            {
                float xCoordinate = hint.XCoordinate;
                float yCoordinate = CoordinateTools.GetVOffset(hint) - yOffset;

                if (xCoordinate != 0) _richTextBuilder.AppendFormat("<pos={0:0.#}>", xCoordinate);
                if (hint.Alignment != HintAlignment.Center) _richTextBuilder.AppendFormat("<align={0}>", hint.Alignment);
                _richTextBuilder.Append("<line-height=0>");
                if (yCoordinate != 0) _richTextBuilder.AppendFormat("<voffset={0:0.#}>", yCoordinate);
                _richTextBuilder.AppendFormat("<size={0}>", hint.FontSize);

                _richTextBuilder.Append(line);

                _richTextBuilder.Append("</size>");
                if (yCoordinate != 0) _richTextBuilder.Append("</voffset>");
                if (hint.Alignment != HintAlignment.Center) _richTextBuilder.Append("</align>");
                _richTextBuilder.AppendLine();

                yOffset += hint.FontSize + hint.LineHeight;
            }

            var result = _richTextBuilder.ToString();

            _richTextBuilder.Clear();

            return result;
        }
    }
}

