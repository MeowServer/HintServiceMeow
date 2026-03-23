namespace HintServiceMeow.Core.Utilities.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Models.Parser.Style;
    using HintServiceMeow.Core.Models.Parser.ValueObject;
    using HintServiceMeow.Core.Models.Transition;
    using HintServiceMeow.Core.Models.UnityAdaptors.Parameters;
    using HintServiceMeow.Core.Utilities.Pools;
    using HintServiceMeow.Core.Utilities.Tools;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;

    /// <summary>
    /// Used to parse AbstractHint to rich text message.
    /// </summary>
    internal class HintParser : IHintParser
    {
        private const string PlaceholderTop = "<line-height=0><voffset=9999>P</voffset>";
        private const string PlaceholderBottom = "<line-height=0><voffset=-9999>P</voffset>";

        private readonly ICache<Guid, ValueTuple<float, float>> dynamicHintPositionCache;
        private readonly ICoordinateTools coordinateTool;
        private readonly IPool<StringBuilder> stringBuilderPool;
        private readonly IPool<RichTextParser> richTextParserPool;
        private readonly IPool<Hint> hintPool;
        private readonly List<Hint> rentedHints = new List<Hint>(128);

        // For ParseToMessage method
        private readonly List<TextArea> dynamicHintColliders = new(128);
        private readonly List<Hint> orderedHintGroups = new(128); // Use Null to seperate groups
        private readonly List<HintSortData> sortBuffer = new(128);
        private readonly List<Hint> orderedHints = new(128);
        private readonly List<DynamicHint> dynamicHints = new(128);

        // For ParseToHint method
        private readonly Queue<ValueTuple<float, float>> queue = new();
        private readonly HashSet<ValueTuple<float, float>> visited = new();

        // For hint parameter handling
        private int parameterIndex = 0;
        private readonly List<IParameter> hintParameters = new(128);

        // For animation
        private string formatString = "F1"; // 1 decimal place
        private bool useIntegral = false; // Use float or int for animated value

        // For ParseToRichText method
        private RichTextParserSetting settingTemplate = new RichTextParserSetting(TextMeshStyle.Default, Array.Empty<Tuple<string, IParameter>>(), ["line-height"],
            ["a", "allcaps", "alpha", "b", "color", "font", "font-weight", "gradient", "i", "lowercase", "mark", "noparse", "s", "smallcaps", "style", "u", "uppercase", "link"],
            true); // Tags that does not affect the size of the text are ignored.

        public HintParser(
            ICache<Guid, ValueTuple<float, float>>? dynamicHintPositionCache = null,
            ICoordinateTools? coordinateTool = null,
            IPool<StringBuilder>? stringBuilderPool = null,
            IPool<RichTextParser>? richTextParserPool = null,
            IPool<Hint>? hintPool = null)
        {
            this.dynamicHintPositionCache = dynamicHintPositionCache ?? new Cache<Guid, (float, float)>(500);
            this.coordinateTool = coordinateTool ?? new CoordinateTools();
            this.stringBuilderPool = stringBuilderPool ?? StringBuilderPool.Instance;
            this.richTextParserPool = richTextParserPool ?? RichTextParserPool.Instance;
            this.hintPool = hintPool ?? HintPool.Instance;
        }

        public HintParserResult ParseToMessage(HintParserArgument arg)
        {
            IReadOnlyList<IReadOnlyList<AbstractHint>> allGroups = arg.Collection.AllGroups;

            Logger.Instance.Debug($"[HintParser] Start parsing hints. Total groups: {allGroups.Count}. Screen Ratio (X/Y): {arg.ScreenXyRatio}.");

            for (int i = 0; i < allGroups.Count; i++)
            {
                for (int j = 0; j < allGroups[i].Count; j++)
                {
                    if (allGroups[i][j] is Hint { Hide: false } hint && !string.IsNullOrEmpty(hint.Content.GetText()))
                        dynamicHintColliders.Add(ParseToArea(hint, arg.ScreenXyRatio));
                }
            }

            for (int i = 0; i < allGroups.Count; i++)
            {
                if (allGroups[i].Count == 0)
                {
                    continue; // Don't add empty group
                }

                for (int j = 0; j < allGroups[i].Count; j++)
                {
                    // Filter invisible hints
                    if (allGroups[i][j] is null || allGroups[i][j].Hide || string.IsNullOrEmpty(allGroups[i][j].Content.GetText()))
                        continue;

                    if (allGroups[i][j] is Hint s)
                        orderedHints.Add(s);
                    else if (allGroups[i][j] is DynamicHint d)
                        dynamicHints.Add(d);
                }

                // Convert Dynamic Hint
                Comparison<DynamicHint> dynamicHintPriorityComparer = (a, b) => b.Priority - a.Priority;
                dynamicHints.Sort(dynamicHintPriorityComparer);

                for (int j = 0; j < dynamicHints.Count; j++)
                {
                    Hint? handledDH = ParseToHint(dynamicHints[j], dynamicHintColliders, arg.ScreenXyRatio);

                    if (handledDH is null)
                        continue;

                    dynamicHintColliders.Add(ParseToArea(handledDH, arg.ScreenXyRatio));
                    orderedHints.Add(handledDH);
                }

                for (int j = 0; j < orderedHints.Count; j++)
                {
                    sortBuffer.Add(new HintSortData(orderedHints[j], coordinateTool.GetYCoordinate(orderedHints[j], HintVerticalAlign.Bottom)));
                }

                // Sort and add to ordered hint groups
                sortBuffer.Sort();

                for (int j = 0; j < sortBuffer.Count; j++)
                {
                    orderedHintGroups.Add(sortBuffer[j].Hint);
                }

                orderedHintGroups.Add(null!);

                // Reset buffers for next group
                orderedHints.Clear();
                dynamicHints.Clear();
                sortBuffer.Clear();
            }

            StringBuilder messageBuilder = stringBuilderPool.Rent();

            messageBuilder.AppendLine(PlaceholderTop); // Place Holder

            for (int i = 0; i < orderedHintGroups.Count; i++)
            {
                // When a group ends
                if (orderedHintGroups[i] is null)
                {
                    continue;
                }

                ParseToRichText(orderedHintGroups[i], messageBuilder, arg.ScreenXyRatio);
            }

            messageBuilder.AppendLine(PlaceholderBottom); // Place Holder

            // Create result
            HintParserResult result = new HintParserResult(messageBuilder.ToString(), hintParameters.ToArray());

            // Clear buffer
            stringBuilderPool.Return(messageBuilder);
            Clear();

            return result;
        }

        private float GetActualX(float rawX, float xyRatio, HintAlignment align, ResolutionOption option)
        {
            switch (option)
            {
                case ResolutionOption.None:
                    return rawX;
                case ResolutionOption.Offset:
                    return rawX + coordinateTool.GetEdgeOffset(xyRatio, align);
                default:
                    throw new NotImplementedException();
            }
        }

        private Hint? ParseToHint(DynamicHint dynamicHint, IList<TextArea> colliders, float xyRatio)
        {
            float dhWidth = coordinateTool.GetTextWidth(dynamicHint);
            float dhHeight = coordinateTool.GetTextHeight(dynamicHint);

            // Check target position before checking the cache
            float actualTargetX = GetActualX(dynamicHint.TargetX, xyRatio, HintAlignment.Center, dynamicHint.ResolutionOption);
            ValueTuple<float, float> targetCoordinate = (actualTargetX, dynamicHint.TargetY);
            TextArea targetArea = DynamicHintToArea(targetCoordinate);

            bool targetAreaAvailable = true;

            for (int i = 0; i < colliders.Count; i++)
            {
                if (targetArea.HasIntersection(colliders[i]))
                {
                    targetAreaAvailable = false;
                    break;
                }
            }

            if (targetAreaAvailable)
            {
                // Clear previous cached position since the target position is usable again
                dynamicHintPositionCache.TryRemove(dynamicHint.Guid, out _);

                Hint hint = hintPool.Rent();
                rentedHints.Add(hint);
                hint.GetFromDynamicHint(dynamicHint, actualTargetX, dynamicHint.TargetY);
                return hint;
            }

            targetAreaAvailable = true;
            if (dynamicHintPositionCache.TryGet(dynamicHint.Guid, out ValueTuple<float, float> cachedPosition))
            {
                TextArea dhArea = DynamicHintToArea(cachedPosition);

                for (int i = 0; i < colliders.Count; i++)
                {
                    if (dhArea.HasIntersection(colliders[i]))
                    {
                        targetAreaAvailable = false;
                        break;
                    }
                }

                if (targetAreaAvailable)
                {
                    Hint hint = hintPool.Rent();
                    rentedHints.Add(hint);
                    hint.GetFromDynamicHint(dynamicHint, cachedPosition.Item1, cachedPosition.Item2);
                    return hint;
                }
            }

            // If there's no cached position or cached position is not usable, then find new position
            queue.Clear();
            visited.Clear();

            queue.Enqueue(targetCoordinate);

            while (queue.TryDequeue(out ValueTuple<float, float> tuple))
            {
                // The tuple represent bottom center coordinate, Item 1: x, Item 2: y
                if (!visited.Add(tuple))
                    continue;

                TextArea dhArea = DynamicHintToArea(tuple);

                targetAreaAvailable = true;
                for (int i = 0; i < colliders.Count; i++)
                {
                    if (dhArea.HasIntersection(colliders[i]))
                    {
                        targetAreaAvailable = false;
                        break;
                    }
                }

                if (targetAreaAvailable)
                {
                    dynamicHintPositionCache.Add(dynamicHint.Guid, tuple);

                    Hint hint = hintPool.Rent();
                    rentedHints.Add(hint);
                    hint.GetFromDynamicHint(dynamicHint, tuple.Item1, tuple.Item2);
                    return hint;
                }

                if (tuple.Item2 < dynamicHint.BottomBoundary)
                    queue.Enqueue((tuple.Item1, tuple.Item2 + 10));
                if (tuple.Item2 > dynamicHint.TopBoundary)
                    queue.Enqueue((tuple.Item1, tuple.Item2 - 10));
                if (tuple.Item1 < dynamicHint.RightBoundary)
                    queue.Enqueue((tuple.Item1 + 50, tuple.Item2));
                if (tuple.Item1 > dynamicHint.LeftBoundary)
                    queue.Enqueue((tuple.Item1 - 50, tuple.Item2));
            }

            // Failed to find a position, return according to DynamicHintStrategy
            if (dynamicHint.Strategy == DynamicHintStrategy.StayInPosition)
            {
                Hint hint = hintPool.Rent();
                rentedHints.Add(hint);
                hint.GetFromDynamicHint(dynamicHint, actualTargetX, dynamicHint.TargetY);
                return hint;
            }

            // DynamicHintStrategy.Hide
            return null;

            TextArea DynamicHintToArea(ValueTuple<float, float> tuple) =>
                new()
                {
                    Left = tuple.Item1 - (dhWidth / 2) - dynamicHint.LeftMargin,
                    Right = tuple.Item1 + (dhWidth / 2) + dynamicHint.RightMargin,
                    Top = tuple.Item2 - dhHeight - dynamicHint.TopMargin,
                    Bottom = tuple.Item2 + dynamicHint.BottomMargin,
                };
        }

        private TextArea ParseToArea(Hint hint, float xyRatio)
        {
            float xCoordinate = GetActualX(coordinateTool.GetXCoordinateWithAlignment(hint), xyRatio, hint.Alignment, hint.ResolutionOption);
            float yCoordinate = coordinateTool.GetYCoordinate(hint, HintVerticalAlign.Bottom);

            float width = coordinateTool.GetTextWidth(hint);
            float height = coordinateTool.GetTextHeight(hint);

            return new TextArea
            {
                Top = yCoordinate - height,
                Bottom = yCoordinate,
                Left = xCoordinate - (width / 2),
                Right = xCoordinate + (width / 2),
            };
        }

        private float GetVOffset(Hint hint, HintVerticalAlign align)
        {
            return 700
                - coordinateTool.GetYCoordinate(hint, align)// Start at the top of the first line
                + hint.LineHeight;// Add extra line height on top of the first line so that the line height will not be calculated for the first line
        }

        private float GetCurrentVOffset(Hint hint, HintVerticalAlign align)
        {
            float yCoordinate = hint.VOffsetTransitionState == null ?
                hint.YCoordinate
                : coordinateTool.GetYCoordinate(hint.VOffsetTransitionState.CurrentValue);

            return 700
                - coordinateTool.GetYCoordinate(yCoordinate, coordinateTool.GetTextHeight(hint), hint.YCoordinateAlign, HintVerticalAlign.Top)// Start at the top of the first line
                + hint.LineHeight;// Add extra line height on top of the first line so that the line height will not be calculated for the first line
        }

        private void ParseToRichText(Hint hint, StringBuilder messageBuilder, float xyRatio)
        {
            // Parse into line infos
            RichTextParser parser = richTextParserPool.Rent();
            settingTemplate.Parameters = hint.Parameters.ToArray();
            settingTemplate.DefaultStyle.CharStyle.FontSize = hint.FontSize;
            settingTemplate.DefaultStyle.LineStyle.Alignment = hint.Alignment;
            RichTextParserResult result = parser.ParseText(hint.Content.GetText() ?? string.Empty, settingTemplate);
            richTextParserPool.Return(parser);

            // Offset parameter index
            parameterIndex += result.ParameterIndex;

            // Add pamameters used in hint content to the list
            hintParameters.AddRange(result.Parameters);

            if (result.LineInfos.Length == 0)
                return;

            // Add default size/alignment
            messageBuilder.Append("<size=");
            hint.FontSizeTransitionState = AddTransition(messageBuilder, hint.CurrentFontSize, hint.FontSize, hint.FontSizeTransition, hint.FontSizeTransitionState);
            messageBuilder.Append('>');

            switch (hint.Alignment)
            {
                case HintAlignment.Left: messageBuilder.Append("<align=left>"); break;
                case HintAlignment.Right: messageBuilder.Append("<align=right>"); break;
            }

            // Get the bottom y coordinate of first line
            float vOffset = GetVOffset(hint, HintVerticalAlign.Top);

            // Get the current v offset
            float fromVOffset = GetCurrentVOffset(hint, HintVerticalAlign.Top);

            bool coordinateTransitionStateAdded = false;

            for (int i = 0; i < result.LineInfos.Length; i++)
            {
                vOffset -= result.LineInfos[i].Height + hint.LineHeight; // Move y coordinate to the bottom of the line
                fromVOffset -= result.LineInfos[i].Height + hint.LineHeight; // Move from coordinate to the bottom of the line

                if (string.IsNullOrEmpty(result.LineInfos[i].CleanText))
                    continue;

                // X coordinate
                float actualXCoordinate = GetActualX(hint.XCoordinate, xyRatio, hint.Alignment, hint.ResolutionOption);
                messageBuilder.Append("<pos=");
                if (!coordinateTransitionStateAdded)// Only add transition state for the first line
                    hint.XCoordinateTransitionState = AddTransition(messageBuilder, hint.CurrentXCoordinate, actualXCoordinate, hint.XCoordinateTransition, hint.XCoordinateTransitionState);
                else
                    AddTransition(messageBuilder, hint.CurrentXCoordinate, actualXCoordinate, hint.XCoordinateTransition, hint.XCoordinateTransitionState);
                messageBuilder.Append('>');

                messageBuilder.Append("<line-height=0>"); // Make sure each line will not affect each other's position

                // Y coordinate
                messageBuilder.Append("<voffset=");
                if (!coordinateTransitionStateAdded)// Only add transition state for the first line
                    hint.VOffsetTransitionState = AddTransition(messageBuilder, fromVOffset, vOffset, hint.YCoordinateTransition, hint.VOffsetTransitionState);
                else
                    AddTransition(messageBuilder, fromVOffset, vOffset, hint.YCoordinateTransition, hint.VOffsetTransitionState);
                messageBuilder.Append('>');

                coordinateTransitionStateAdded = true;

                messageBuilder.Append(result.LineInfos[i].CleanText); // Content

                messageBuilder.Append("</voffset>"); // End Y coordinate

                messageBuilder.AppendLine(); // Break line
            }

            // End default alignment/size
            if (hint.Alignment != HintAlignment.Center)
                messageBuilder.Append("</align>");
            messageBuilder.Append("</size>");
        }

        private void Clear()
        {
            for (int i = 0; i < rentedHints.Count; i++)// Return rented hints to pool
            {
                hintPool.Return(rentedHints[i]);
            }

            rentedHints.Clear();
            orderedHintGroups.Clear();
            dynamicHintColliders.Clear();
            parameterIndex = 0;
            hintParameters.Clear();
        }

        private TransitionState? AddTransition(
            StringBuilder sb,
            float from,
            float to,
            Transition? transition,
            TransitionState? transitionState)
        {
            // If there's no transition, just return the target value
            if (transition is null)
            {
                sb.Append(to.ToString(formatString));
                return null;
            }

            // Check if previous transition state is aiming for same target value.
            // If so, we believe that they are same transition. Keep the same transition.
            double startTime = NetworkTimeCache.Time;
            if (transitionState is not null
                && transitionState.ToValue == to)
            {
                startTime = transitionState.StartTime;
                from = transitionState.FromValue;
            }

            // If target value is different, start a new transition from current value.
            IAnimationCurve curve = transition.GetCurve(from, to);
            sb.Append('{').Append(parameterIndex).Append('}');
            parameterIndex++;
            hintParameters.Add(new AnimationParameter(startTime, curve, formatString, useIntegral));

            return new TransitionState(transition, from, to, startTime);
        }
    }
}