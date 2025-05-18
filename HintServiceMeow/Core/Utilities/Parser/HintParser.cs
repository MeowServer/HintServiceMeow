using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Core.Utilities.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HintServiceMeow.Core.Utilities.Parser
{
    /// <summary>
    /// Used to parse AbstractHint to rich text message
    /// </summary>
    internal class HintParser : IHintParser
    {
        private static readonly Regex IllegalTagRegex = new(
            @"<line-height=[^>]*>|<voffset=[^>]*>|<pos=[^>]*>|</voffset>|{|}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Cache<Guid, ValueTuple<float, float>> _dynamicHintPositionCache = new(500);

        public string ParseToMessage(HintCollection collection)
        {
            IReadOnlyList<IReadOnlyList<AbstractHint>> allGroups = collection.AllGroups;

            List<TextArea> dynamicHintColliders = new List<TextArea>();
            foreach (AbstractHint h in allGroups.SelectMany(g => g))
            {
                if (h is Hint hint && !hint.Hide && !string.IsNullOrEmpty(hint.Content.GetText()))
                    dynamicHintColliders.Add(ParseToArea(hint));
            }

            List<List<Hint>> orderedHintGroups = new();

            foreach (IReadOnlyList<AbstractHint> group in allGroups)
            {
                //Group by type
                List<Hint> orderedHints = new List<Hint>();
                List<DynamicHint> dynamicHints = new List<DynamicHint>();

                foreach (var item in group)
                {
                    //Filter invisible hints
                    if (item is null || item.Hide || string.IsNullOrEmpty(item.Content.GetText()))
                        continue;

                    if (item is Hint s)
                        orderedHints.Add(s);
                    else if (item is DynamicHint d)
                        dynamicHints.Add(d);
                }

                //Convert Dynamic Hint
                if (dynamicHints.Any())
                {
                    dynamicHints.Sort((a, b) => b.Priority - a.Priority);

                    foreach (DynamicHint dynamicHint in dynamicHints)
                    {
                        Hint handledDH = ParseToHint(dynamicHint, dynamicHintColliders);

                        if (handledDH is null)
                            continue;

                        dynamicHintColliders.Add(ParseToArea(handledDH));
                        orderedHints.Add(handledDH);
                    }
                }

                List<(Hint hint, float y)> temp = orderedHints
                    .Select(h => (hint: h, y: CoordinateTools.GetYCoordinate(h, HintVerticalAlign.Bottom)))
                    .ToList();

                temp.Sort((a, b) => a.y.CompareTo(b.y));

                List<Hint> result = temp.Select(x => x.hint).ToList();

                //Sort and add to ordered hint groups
                orderedHintGroups.Add(result);
            }

            StringBuilder messageBuilder = StringBuilderPool.Rent(5000);
            const int NetLimit = 65000;

            messageBuilder.AppendLine("<line-height=0><voffset=9999>P</voffset>");//Place Holder

            foreach (List<Hint> hintList in orderedHintGroups)
            {
                if (hintList.IsEmpty())
                    continue;

                foreach (Hint hint in hintList)
                {
                    if (messageBuilder.Length > NetLimit)
                        break; //Prevent network message from overflow

                    string text = ParseToRichText(hint);
                    if (!string.IsNullOrEmpty(text))
                        messageBuilder.Append(text); //ToRichText already added \n at the end
                }

                if (messageBuilder.Length > NetLimit)
                    break; //Prevent network message from overflow

                messageBuilder.AppendLine("</align></size></b></i>"); //Make sure one group will not affect another group
            }

            messageBuilder.AppendLine("<line-height=0><voffset=-9999>P</voffset>");//Place Holder
            return StringBuilderPool.ToStringReturn(messageBuilder);
        }

        private Hint ParseToHint(DynamicHint dynamicHint, IList<TextArea> colliders)
        {
            float dhWidth = CoordinateTools.GetTextWidth(dynamicHint);
            float dhHeight = CoordinateTools.GetTextHeight(dynamicHint);

            TextArea DynamicHintToArea(ValueTuple<float, float> tuple) =>
                new()
                {
                    Left = tuple.Item1 - dhWidth / 2 - dynamicHint.LeftMargin,
                    Right = tuple.Item1 + dhWidth / 2 + dynamicHint.RightMargin,
                    Top = tuple.Item2 - dhHeight - dynamicHint.TopMargin,
                    Bottom = tuple.Item2 + dynamicHint.BottomMargin,
                };

            //Check target position before checking the cache
            ValueTuple<float, float> targetCoordinate = ValueTuple.Create(dynamicHint.TargetX, dynamicHint.TargetY);
            TextArea targetArea = DynamicHintToArea(targetCoordinate);
            if (!colliders.Any(targetArea.HasIntersection))
            {
                //Clear previous cached position since the target position is usable again
                _dynamicHintPositionCache.TryRemove(dynamicHint.Guid, out _);

                return new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY);
            }

            if (_dynamicHintPositionCache.TryGet(dynamicHint.Guid, out ValueTuple<float, float> cachedPosition))
            {
                TextArea dhArea = DynamicHintToArea(cachedPosition);
                if (!colliders.Any(dhArea.HasIntersection))
                {
                    return new Hint(dynamicHint, cachedPosition.Item1, cachedPosition.Item2);
                }
            }

            //If there's no cached position or cached position is not usable, then find new position
            Queue<ValueTuple<float, float>> queue = new();
            HashSet<ValueTuple<float, float>> visited = new();

            queue.Enqueue(targetCoordinate);

            while (queue.TryDequeue(out ValueTuple<float, float> tuple))
            {
                //The tuple represent bottom center coordinate, Item 1: x, Item 2: y
                if (!visited.Add(tuple))
                    continue;

                TextArea dhArea = DynamicHintToArea(tuple);
                if (!colliders.Any(dhArea.HasIntersection))
                {
                    _dynamicHintPositionCache.Add(dynamicHint.Guid, tuple);
                    return new Hint(dynamicHint, tuple.Item1, tuple.Item2);
                }

                if (tuple.Item2 < dynamicHint.BottomBoundary)
                    queue.Enqueue(ValueTuple.Create(tuple.Item1, tuple.Item2 + 10));
                if (tuple.Item2 > dynamicHint.TopBoundary)
                    queue.Enqueue(ValueTuple.Create(tuple.Item1, tuple.Item2 - 10));
                if (tuple.Item1 < dynamicHint.RightBoundary)
                    queue.Enqueue(ValueTuple.Create(tuple.Item1 + 50, tuple.Item2));
                if (tuple.Item1 > dynamicHint.LeftBoundary)
                    queue.Enqueue(ValueTuple.Create(tuple.Item1 - 50, tuple.Item2));
            }

            //Failed to find a position, return according to DynamicHintStrategy
            if (dynamicHint.Strategy == DynamicHintStrategy.StayInPosition)
            {
                return new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY);
            }

            // DynamicHintStrategy.Hide
            return null;
        }

        private TextArea ParseToArea(Hint hint)
        {
            float xCoordinate = CoordinateTools.GetXCoordinateWithAlignment(hint);
            float yCoordinate = CoordinateTools.GetYCoordinate(hint, HintVerticalAlign.Bottom);

            float width = CoordinateTools.GetTextWidth(hint);
            float height = CoordinateTools.GetTextHeight(hint);

            return new TextArea
            {
                Top = yCoordinate - height,
                Bottom = yCoordinate,
                Left = xCoordinate - width / 2,
                Right = xCoordinate + width / 2,
            };
        }

        private string ParseToRichText(Hint hint)
        {
            //Remove illegal tags
            string raw = hint.Content.GetText() ?? string.Empty;
            string text = IllegalTagRegex.Replace(raw, string.Empty);

            //Parse into line infos
            IReadOnlyList<LineInfo> lineList = RichTextParserPool.ParseText(text, hint.FontSize);

            if (lineList is null || lineList.IsEmpty())
                return null;

            //Get the bottom y coordinate of first line
            float vOffset =
                700
                - CoordinateTools.GetYCoordinate(hint, HintVerticalAlign.Top)// Start at the top of the first line
                + hint.LineHeight;// Add extra line height on top of the first line so that the line height will not be calculated for the first line

            //Start to generate rich text
            StringBuilder richTextBuilder = StringBuilderPool.Rent(text.Length + 200);

            //Add default size/alignment
            richTextBuilder.AppendFormat("<size={0}>", hint.FontSize);
            if (hint.Alignment != HintAlignment.Center) richTextBuilder.AppendFormat("<align={0}>", hint.Alignment);

            foreach (LineInfo line in lineList)
            {
                vOffset -= line.Height + hint.LineHeight; //Move y coordinate to the bottom of the line

                if (string.IsNullOrEmpty(line.RawText))
                    continue;

                if (hint.XCoordinate != 0) richTextBuilder.AppendFormat("<pos={0:0.#}>", hint.XCoordinate);//X coordinate
                richTextBuilder.Append("<line-height=0>");//Make sure each line will not affect each other's position
                if (vOffset != 0) richTextBuilder.AppendFormat("<voffset={0:0.#}>", vOffset);//Y coordinate

                richTextBuilder.Append(line.RawText);//Content

                if (vOffset != 0) richTextBuilder.Append("</voffset>");//End Y coordinate
                richTextBuilder.AppendLine(); //Break line
            }

            //End default alignment/size
            if (hint.Alignment != HintAlignment.Center) richTextBuilder.Append("</align>");
            richTextBuilder.Append("</size>");

            return StringBuilderPool.ToStringReturn(richTextBuilder);
        }
    }
}