using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Utilities.Parser
{
    /// <summary>
    /// Used to parse AbstractHint to rich text message
    /// </summary>
    internal class HintParser : IHintParser
    {
        private readonly ConcurrentDictionary<Guid, ValueTuple<float, float>> _dynamicHintPositionCache = new ConcurrentDictionary<Guid, ValueTuple<float, float>>();

        public string ParseToMessage(HintCollection collection)
        {
            List<List<Hint>> orderedHintGroups = new List<List<Hint>>();

            List<TextArea> dynamicHintColliders = collection.AllGroups
                .SelectMany(x => x)
                .OfType<Hint>()
                .Where(x => x is not null && !x.Hide && !string.IsNullOrEmpty(x.Content.GetText()))
                .Select(ParseToArea)
                .ToList();

            foreach (var group in collection.AllGroups)
            {
                //Filter invisible hints
                IEnumerable<AbstractHint> visibleGroup = group.Where(x => x is not null && !x.Hide && !string.IsNullOrEmpty(x.Content.GetText()));

                //Group by type
                List<Hint> orderedHints = visibleGroup.OfType<Hint>().ToList();
                List<DynamicHint> dynamicHints = visibleGroup.OfType<DynamicHint>().ToList();

                //Convert Dynamic Hint
                dynamicHints.Sort((a, b) => b.Priority - a.Priority);
                foreach (var dynamicHint in dynamicHints)
                {
                    Hint handledDH = ParseToHint(dynamicHint, dynamicHintColliders);

                    if (handledDH == null)
                        continue;

                    dynamicHintColliders.Add(ParseToArea(handledDH));
                    orderedHints.Add(handledDH);
                }

                //Sort and add to ordered hint groups
                orderedHints.Sort((x, y) => CoordinateTools.GetYCoordinate(x, HintVerticalAlign.Bottom).CompareTo(CoordinateTools.GetYCoordinate(y, HintVerticalAlign.Bottom)));
                orderedHintGroups.Add(orderedHints);
            }

            StringBuilder messageBuilder = StringBuilderPool.Rent(5000);

            messageBuilder.AppendLine("<line-height=0><voffset=9999>P</voffset>");//Place Holder

            foreach (List<Hint> hintList in orderedHintGroups)
            {
                foreach (Hint hint in hintList)
                {
                    if (messageBuilder.Length > 65400)
                        break; //Prevent network message from overflow

                    string text = ParseToRichText(hint);
                    if (!string.IsNullOrEmpty(text))
                        messageBuilder.Append(text); //ToRichText already added \n at the end
                }

                if (!hintList.IsEmpty())
                    messageBuilder.AppendLine("</align></size></b></i>"); //Make sure one group will not affect another group
            }
            
            messageBuilder.AppendLine("<line-height=0><voffset=-9999>P</voffset>");//Place Holder

            return StringBuilderPool.ToStringReturn(messageBuilder);
        }

        private Hint ParseToHint(DynamicHint dynamicHint, IEnumerable<TextArea> colliders)
        {
            var dhWidth = CoordinateTools.GetTextWidth(dynamicHint);
            var dhHeight = CoordinateTools.GetTextHeight(dynamicHint);

            TextArea DynamicHintToArea(ValueTuple<float, float> tuple)
            {
                return new TextArea
                {
                    Left = tuple.Item1 - dhWidth / 2 - dynamicHint.LeftMargin,
                    Right = tuple.Item1 + dhWidth / 2 + dynamicHint.RightMargin,
                    Top = tuple.Item2 - dhHeight - dynamicHint.TopMargin,
                    Bottom = tuple.Item2 + dynamicHint.BottomMargin,
                };
            }

            //Check target position before checking the cache
            var targetArea = DynamicHintToArea(ValueTuple.Create(dynamicHint.TargetX, dynamicHint.TargetY));
            if (!colliders.Any(targetArea.HasIntersection))
            {
                //Clear previous cached position since the target position is usable again
                _dynamicHintPositionCache.TryRemove(dynamicHint.Guid, out _);

                return new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY);
            }

            if (_dynamicHintPositionCache.TryGetValue(dynamicHint.Guid, out var cachedPosition))
            {
                var dhArea = DynamicHintToArea(cachedPosition);
                if (!colliders.Any(dhArea.HasIntersection))
                {
                    return new Hint(dynamicHint, cachedPosition.Item1, cachedPosition.Item2);
                }
            }

            //If there's no cached position or cached position is not usable, then find new position
            var queue = new Queue<ValueTuple<float, float>>();
            var visited = new HashSet<ValueTuple<float, float>>();

            queue.Enqueue(ValueTuple.Create(dynamicHint.TargetX, dynamicHint.TargetY));

            while (queue.TryDequeue(out var tuple))
            {
                //The tuple represent bottom center coordinate, Item 1: x, Item 2: y
                if (visited.Contains(tuple))
                    continue;

                visited.Add(tuple);

                var dhArea = DynamicHintToArea(tuple);
                if (!colliders.Any(dhArea.HasIntersection))
                {
                    _dynamicHintPositionCache[dynamicHint.Guid] = tuple;

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
            return dynamicHint.Strategy switch
            {
                DynamicHintStrategy.StayInPosition => new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY),
                DynamicHintStrategy.Hide => null,
                _ => null
            };
        }

        private TextArea ParseToArea(Hint hint)
        {
            var xCoordinate = CoordinateTools.GetXCoordinateWithAlignment(hint);
            var yCoordinate = CoordinateTools.GetYCoordinate(hint, HintVerticalAlign.Bottom);

            return new TextArea
            {
                Top = yCoordinate - CoordinateTools.GetTextHeight(hint),
                Bottom = yCoordinate,
                Left = xCoordinate - CoordinateTools.GetTextWidth(hint) / 2,
                Right = xCoordinate + CoordinateTools.GetTextWidth(hint) / 2,
            };
        }

        private string ParseToRichText(Hint hint)
        {
            //Remove illegal tags
            string text = Regex.Replace(
                    hint.Content.GetText() ?? string.Empty,
                    @"<line-height=[^>]*>|<voffset=[^>]*>|<pos=[^>]*>|</voffset>|{|}",
                    string.Empty,
                    RegexOptions.IgnoreCase | RegexOptions.Compiled
                );

            //Parse into line infos
            var lineList = RichTextParserPool.ParseText(text, hint.FontSize);

            if (lineList is null || lineList.IsEmpty())
                return null;

            //Get the bottom y coordinate of first line
            float vOffset =
                700
                - CoordinateTools.GetYCoordinate(hint, HintVerticalAlign.Top)// Start at the top of the first line
                + hint.LineHeight;// Add extra line height on top of the first line so that the line height will not be calculate for the first line

            //Start to generate rich text
            StringBuilder richTextBuilder = StringBuilderPool.Rent(text.Length + 200);

            //Add default size/alignment
            richTextBuilder.AppendFormat("<size={0}>", hint.FontSize);
            if (hint.Alignment != HintAlignment.Center) richTextBuilder.AppendFormat("<align={0}>", hint.Alignment);

            foreach (var line in lineList)
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

            //End default size/alignment
            if (hint.Alignment != HintAlignment.Center) richTextBuilder.Append("</align>");
            richTextBuilder.Append("</size>");

            return StringBuilderPool.ToStringReturn(richTextBuilder);
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

