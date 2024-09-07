using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Tools;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Used to parse AbstractHint to rich text message
    /// </summary>
    internal class HintParser
    {
        private readonly RichTextParser _richTextParser = new RichTextParser();

        private readonly StringBuilder _richTextBuilder = new StringBuilder(500); //Used to build rich text from Hint
        private readonly object _richTextLock = new object();

        private readonly StringBuilder _messageBuilder = new StringBuilder(ushort.MaxValue);
        private readonly object _messageBuilderLock = new object();

        private readonly List<List<Hint>> _hintList = new List<List<Hint>>();
        private readonly object _hintListLock = new object();

        private readonly Dictionary<Guid, ValueTuple<float, float>> _dynamicHintPositionCache = new Dictionary<Guid, ValueTuple<float, float>>();
        private readonly object _dynamicHintCacheLock = new object();

        public string GetMessage(HintCollection collection)
        {
            string result;

            lock (_hintListLock)
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
                            orderedList.Add(ConvertDynamicHint(dynamicHint, collection.AllHints.OfType<Hint>().Concat(orderedList)));
                    }

                    //Sort by y coordinate and priority
                    orderedList.Sort((x, y) => CoordinateTools.GetYCoordinate(_richTextParser, x, HintVerticalAlign.Bottom).CompareTo(CoordinateTools.GetYCoordinate(_richTextParser, y, HintVerticalAlign.Bottom)));

                    _hintList.Add(orderedList);
                }

                lock (_messageBuilderLock)
                {
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

                    result = _messageBuilder.ToString();
                    _messageBuilder.Clear();
                }
            }

            //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{DateTime.Now.Ticks}.txt");
            //File.WriteAllText(path, result);

            return result;
        }

        private Hint ConvertDynamicHint(DynamicHint dynamicHint, IEnumerable<Hint> existingHints)
        {
            List<TextArea> textAreas = existingHints.
                Select(hint =>
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

            bool HasIntersection(float x, float y)
            {
                return textAreas.Any(hintArea =>
                {
                    var dhArea = new TextArea
                    {
                        Left = x - CoordinateTools.GetTextWidth(_richTextParser, dynamicHint) / 2,
                        Right = x + CoordinateTools.GetTextWidth(_richTextParser, dynamicHint) / 2,
                        Top = y - CoordinateTools.GetTextHeight(_richTextParser, dynamicHint),
                        Bottom = y,
                    };

                    return dhArea.HasIntersection(hintArea);
                });
            }

            bool cached;
            ValueTuple<float, float> cachedPosition;

            lock (_dynamicHintCacheLock)
                cached = _dynamicHintPositionCache.TryGetValue(dynamicHint.Guid, out cachedPosition);

            if (cached)
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
                    lock(_dynamicHintCacheLock)
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

            var lineList = SplitIntoLines(text, hint.FontSize);

            var splitText = string.Join("\n", lineList);
            var newTextHeight = CoordinateTools.GetTextHeight(_richTextParser, splitText, hint.FontSize, hint.LineHeight);

            lineList.Reverse();

            //Building rich text by lines
            float yCoordinate = 700 - CoordinateTools.GetYCoordinate(hint.YCoordinate, newTextHeight, hint.YCoordinateAlign, HintVerticalAlign.Bottom); //Get the top coordinate as pivot

            string result;

            lock (_richTextLock)
            {
                _richTextBuilder.Clear();

                foreach (var line in lineList)
                {
                    var lineText = line;

                    if (string.IsNullOrEmpty(lineText))
                    {
                        lineText = " "; // For height calculation
                    }
                    else
                    {
                        float xCoordinate = hint.XCoordinate;
                        

                        if (xCoordinate != 0) _richTextBuilder.AppendFormat("<pos={0:0.#}>", xCoordinate);
                        if (hint.Alignment != HintAlignment.Center) _richTextBuilder.AppendFormat("<align={0}>", hint.Alignment);
                        _richTextBuilder.Append("<line-height=0>");
                        if (yCoordinate != 0) _richTextBuilder.AppendFormat("<voffset={0:0.#}>", yCoordinate);
                        _richTextBuilder.AppendFormat("<size={0}>", hint.FontSize);

                        _richTextBuilder.Append(lineText);

                        _richTextBuilder.Append("</size>");
                        if (yCoordinate != 0) _richTextBuilder.Append("</voffset>");
                        if (hint.Alignment != HintAlignment.Center) _richTextBuilder.Append("</align>");

                        _richTextBuilder.AppendLine();
                    }

                    yCoordinate += CoordinateTools.GetTextHeight(_richTextParser, lineText, hint.FontSize, hint.LineHeight);
                }

                result = _richTextBuilder.ToString();

                _richTextBuilder.Clear();
            }

            return result;
        }

        private List<string> SplitIntoLines(string text, int fontSize)//Split based on \n and width
        {
            List<string> lines = new List<string>();

            foreach (var line in text.Split('\n'))
            {
                if(string.IsNullOrEmpty(line))
                {
                    lines.Add(string.Empty);
                    continue;
                }

                string currentPart = string.Empty;
                Regex tagRegex = new Regex(@"<[^>]+>");

                int i = 0;
                while (i < line.Length)
                {
                    Match tagMatch;
                    if (line[i] == '<' && (tagMatch = tagRegex.Match(line, i)).Success)
                    {
                        currentPart += tagMatch.Value;
                        i += tagMatch.Length;
                    }
                    else
                    {
                        currentPart += line[i];
                        string partWithoutTags = tagRegex.Replace(currentPart, string.Empty);

                        if (CoordinateTools.GetTextWidth(_richTextParser, partWithoutTags, fontSize) > 2400)
                        {
                            lines.Add(currentPart.Substring(0, currentPart.Length - 1));
                            currentPart = line[i].ToString();
                        }

                        i++;
                    }
                }

                if (currentPart.Length > 0)
                {
                    lines.Add(currentPart);
                }
            }

            return lines;
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

