using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Utilities.Parser
{
    /// <summary>
    /// Used to parse AbstractHint to rich text message
    /// </summary>
    internal class HintParser
    {
        private readonly RichTextParser _richTextParser = new RichTextParser();

        private readonly StringBuilder _richTextBuilder = new StringBuilder(500); //Used to build rich text from Hint
        private readonly object _richTextLock = new object();

        private readonly ConcurrentDictionary<Guid, ValueTuple<float, float>> _dynamicHintPositionCache = new ConcurrentDictionary<Guid, ValueTuple<float, float>>();

        public string GetMessage(HintCollection collection)
        {
            List<List<Hint>> OrderedHintList = new List<List<Hint>>();

            foreach (var group in collection.AllGroups.ToList())
            {
                List<Hint> unhandledH = new List<Hint>();
                List<DynamicHint> unhandledDH = new List<DynamicHint>();

                //Sort and filter the hint
                foreach (var item in group.ToList())
                {
                    if (item is null || item.Hide || string.IsNullOrEmpty(item.Content.GetText()))
                        continue;

                    if (item is Hint hint)
                        unhandledH.Add(hint);
                    else if (item is DynamicHint dynamicHint)
                        unhandledDH.Add(dynamicHint);
                }

                //Higher priority, more forward to convert
                unhandledDH.Sort((x, y) => y.Priority - x.Priority);

                //Convert dh
                foreach (var dh in unhandledDH)
                    unhandledH.Add(ConvertDynamicHint(dh, unhandledH));

                //Sort by y coordinate
                unhandledH.Sort((x, y) =>
                {
                    var yCoordinateX = CoordinateTools.GetYCoordinate(_richTextParser, x, HintVerticalAlign.Bottom);
                    var yCoordinateY = CoordinateTools.GetYCoordinate(_richTextParser, y, HintVerticalAlign.Bottom);
                    return yCoordinateX.CompareTo(yCoordinateY);
                });

                OrderedHintList.Add(unhandledH);
            }

            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.AppendLine("<line-height=0><voffset=9999>P</voffset>");//Place Holder

            foreach (List<Hint> hintList in OrderedHintList)
            {
                foreach (Hint hint in hintList)
                {
                    var text = ToRichText(hint);
                    if (!string.IsNullOrEmpty(text))
                        messageBuilder.Append(text);//ToRichText already added \n at the end

                    if (messageBuilder.Length + 
                        "</size></b></i>".Length + 
                        "<line-height=0><voffset=-9999>P</voffset>".Length > ushort.MaxValue)
                        break; //Prevent message to be overflow
                }

                if(!hintList.IsEmpty())
                    messageBuilder.AppendLine("</align></size></b></i>"); //Make sure one group will not affect another group
            }

            messageBuilder.AppendLine("<line-height=0><voffset=-9999>P</voffset>");//Place Holder

            //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{DateTime.Now.Ticks}.txt");
            //File.WriteAllText(path, result);

            return messageBuilder.ToString();
        }

        private Hint ConvertDynamicHint(DynamicHint dynamicHint, IEnumerable<Hint> existingHints)
        {
            List<TextArea> textAreas = existingHints
                .Where(hint => hint != null && !hint.Hide && !string.IsNullOrEmpty(hint.Content.GetText()))
                .Select(hint =>
                {
                    var xCoordinate = CoordinateTools.GetXCoordinateWithAlignment(_richTextParser, hint);
                    var yCoordinate = CoordinateTools.GetYCoordinate(_richTextParser, hint, HintVerticalAlign.Bottom);

                    return new TextArea
                    {
                        Top = yCoordinate - CoordinateTools.GetTextHeight(_richTextParser, hint),
                        Bottom = yCoordinate,
                        Left = xCoordinate - CoordinateTools.GetTextWidth(_richTextParser, hint) / 2,
                        Right = xCoordinate + CoordinateTools.GetTextWidth(_richTextParser, hint) / 2,
                    };
                })
                .ToList();

            //Try find cache
            if (_dynamicHintPositionCache.TryGetValue(dynamicHint.Guid, out var cachedPosition))
            {
                var cachedArea = new TextArea
                {
                    Left = cachedPosition.Item1 - CoordinateTools.GetTextWidth(_richTextParser, dynamicHint) / 2 - dynamicHint.LeftMargin,
                    Right = cachedPosition.Item1 + CoordinateTools.GetTextWidth(_richTextParser, dynamicHint) / 2 + dynamicHint.RightMargin,
                    Top = cachedPosition.Item2 - CoordinateTools.GetTextHeight(_richTextParser, dynamicHint) - dynamicHint.TopMargin,
                    Bottom = cachedPosition.Item2 + dynamicHint.BottomMargin,
                };

                //If cached position is not intersected with any hint, use it
                if (!textAreas.Any(cachedArea.HasIntersection))
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

                var dhArea = new TextArea
                {
                    Left = x - CoordinateTools.GetTextWidth(_richTextParser, dynamicHint) / 2 - dynamicHint.LeftMargin,
                    Right = x + CoordinateTools.GetTextWidth(_richTextParser, dynamicHint) / 2 + dynamicHint.RightMargin,
                    Top = y - CoordinateTools.GetTextHeight(_richTextParser, dynamicHint) - dynamicHint.TopMargin,
                    Bottom = y + dynamicHint.BottomMargin,
                };

                if (!textAreas.Any(dhArea.HasIntersection))
                {
                    //Found a position that does not overlap with any hint. Add into cache
                    _dynamicHintPositionCache[dynamicHint.Guid] = ValueTuple.Create(x, y);

                    return new Hint(dynamicHint, x, y);
                }

                if(y < dynamicHint.BottomBoundary)
                    queue.Enqueue(ValueTuple.Create(x, y + 10));
                if(y > dynamicHint.TopBoundary)
                    queue.Enqueue(ValueTuple.Create(x, y - 10));
                if (x < dynamicHint.RightBoundary)
                    queue.Enqueue(ValueTuple.Create(x + 50, y));
                if (x > dynamicHint.LeftBoundary)
                    queue.Enqueue(ValueTuple.Create(x - 50, y));
            }

            if (dynamicHint.Strategy == DynamicHintStrategy.StayInPosition)
                return new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY);

            return null;
        }

        private string ToRichText(Hint hint)
        {
            //Remove Illegal Tags
            string text = hint.Content.GetText() ?? string.Empty;

            //Remove illegal tags
            text = Regex
                .Replace(
                    text,
                    @"<line-height=[^>]*>|<voffset=[^>]*>|<pos=[^>]*>|<align=[^>]*>|</voffset>|</align>|{|}",
                    string.Empty,
                    RegexOptions.IgnoreCase | RegexOptions.Compiled
                );

            if (string.IsNullOrEmpty(text))
                return null;

            var lineList = _richTextParser.ParseText(text, hint.FontSize);

            if(lineList.IsEmpty())
                return null;

            //Get the bottom y coordinate of first line
            float vOffset = 
                700
                - CoordinateTools.GetYCoordinate(hint.YCoordinate, lineList.Sum(x => x.Height), hint.YCoordinateAlign, HintVerticalAlign.Top)
                - lineList.First().Height;

            string result;

            lock (_richTextLock)
            {
                _richTextBuilder.Clear();

                _richTextBuilder.AppendFormat("<size={0}>", hint.FontSize);
                if (hint.Alignment != HintAlignment.Center) _richTextBuilder.AppendFormat("<align={0}>", hint.Alignment);
                foreach (var line in lineList)
                {
                    var lineText = line.RawText;

                    if (string.IsNullOrEmpty(lineText))
                    {
                        lineText = " "; // For height calculation
                    }
                    else if(line.Characters.Count > 0)
                    {
                        float xCoordinate = hint.XCoordinate;

                        if (xCoordinate != 0) _richTextBuilder.AppendFormat("<pos={0:0.#}>", xCoordinate);
                        
                        _richTextBuilder.Append("<line-height=0>");
                        if (vOffset != 0) _richTextBuilder.AppendFormat("<voffset={0:0.#}>", vOffset);

                        _richTextBuilder.Append(lineText);

                        if (vOffset != 0) _richTextBuilder.Append("</voffset>");
                        

                        _richTextBuilder.AppendLine();
                    }

                    vOffset -= CoordinateTools.GetTextHeight(_richTextParser, lineText, hint.FontSize) + hint.LineHeight;
                }
                if (hint.Alignment != HintAlignment.Center) _richTextBuilder.Append("</align>");
                _richTextBuilder.Append("</size>");

                result = _richTextBuilder.ToString();
                _richTextBuilder.Clear();
            }

            return result;
        }

        private class TextArea
        {
            public float Top;

            public float Bottom;

            public float Left;

            public float Right;

            public bool HasIntersection(TextArea area)
            {
                return !(Left >= area.Right || area.Left >= Right || Top >= area.Bottom || area.Top >= Bottom);
            }
        }
    }
}

