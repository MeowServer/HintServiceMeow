namespace HintServiceMeow.Core.Utilities.Tools
{
    using System;
    using System.Collections.Generic;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Models.Parser;
    using HintServiceMeow.Core.Models.Parser.Style;
    using HintServiceMeow.Core.Utilities.Parser;

    /// <summary>
    /// Used to help calculate coordinate for hints.
    /// </summary>
    internal class CoordinateTools : ICoordinateTools
    {
        private const float CanvasHalfWidth = 1200f;

        private readonly IPool<RichTextParser> richTextParserPool;

        private readonly RichTextParserSetting settingTemplate = new RichTextParserSetting(
            TextMeshStyle.Default,
            [],
            [],
            ["a", "allcaps", "alpha", "b", "color", "font", "font-weight", "gradient", "i", "lowercase", "mark", "noparse", "s", "smallcaps", "style", "sub", "sup", "u", "uppercase", "link"],
            true); // Tags that does not affect the size of the text, so they can be ignored when calculating text size.

        public CoordinateTools(IPool<RichTextParser>? richTextParserPool = null)
        {
            this.richTextParserPool = richTextParserPool ?? Pools.RichTextParserPool.Instance;
        }

        public float GetYCoordinate(Hint hint, HintVerticalAlign to)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetYCoordinate(hint, hint.YCoordinateAlign, to);
        }

        public float GetYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetYCoordinate(hint.YCoordinate, GetTextHeight(hint), from, to);
        }

        public float GetYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to)
        {
            if (from == to)
                return rawYCoordinate;

            float offset = 0;

            switch (from)
            {
                case HintVerticalAlign.Top:
                    offset += textHeight;
                    break;
                case HintVerticalAlign.Middle:
                    offset += textHeight / 2;
                    break;
                case HintVerticalAlign.Bottom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(from), from, null);
            }

            switch (to)
            {
                case HintVerticalAlign.Top:
                    offset -= textHeight;
                    break;
                case HintVerticalAlign.Middle:
                    offset -= textHeight / 2;
                    break;
                case HintVerticalAlign.Bottom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(to), to, null);
            }

            return rawYCoordinate + offset;
        }

        public float GetCurrentYCoordinate(Hint hint, HintVerticalAlign to)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetCurrentYCoordinate(hint, hint.YCoordinateAlign, to);
        }

        public float GetCurrentYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            float yCoordinate = hint.VOffsetTransitionState == null ?
                hint.YCoordinate : GetYCoordinate(hint.VOffsetTransitionState.CurrentValue);

            return GetCurrentYCoordinate(yCoordinate, GetTextHeight(hint), from, to);
        }

        public float GetCurrentYCoordinate(float rawCurrentYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to)
        {
            if (from == to)
                return rawCurrentYCoordinate;

            float offset = 0;

            switch (from)
            {
                case HintVerticalAlign.Top:
                    offset += textHeight;
                    break;
                case HintVerticalAlign.Middle:
                    offset += textHeight / 2;
                    break;
                case HintVerticalAlign.Bottom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(from), from, null);
            }

            switch (to)
            {
                case HintVerticalAlign.Top:
                    offset -= textHeight;
                    break;
                case HintVerticalAlign.Middle:
                    offset -= textHeight / 2;
                    break;
                case HintVerticalAlign.Bottom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(to), to, null);
            }

            return rawCurrentYCoordinate + offset;
        }

        public float GetEdgeOffset(float xyRatio, HintAlignment alignment)
        {
            switch (alignment)
            {
                case HintAlignment.Left:
                    return -(xyRatio * 540) + 600;// (-(xyRatio - (4f / 3f)) * 240f / (4f / 9f)) - 120f; // 4/3 => 120, 16/9 => 360
                case HintAlignment.Right:
                    return 0; // Cannot push thorugh the right edge, otherwise the hint will be cut off by auto line-wrapping. So just align to the right edge.
                case HintAlignment.Center:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }

        public float GetXCoordinateWithAlignment(Hint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetXCoordinateWithAlignment(hint, hint.Alignment);
        }

        public float GetXCoordinateWithAlignment(Hint hint, HintAlignment alignment)
        {
            float width = GetTextWidth(hint);
            float alignOffset = alignment switch
            {
                HintAlignment.Left => -CanvasHalfWidth + (width / 2),
                HintAlignment.Right => CanvasHalfWidth - (width / 2),
                _ => 0,
            };

            return hint.XCoordinate + alignOffset;
        }

        public float GetTextWidth(AbstractHint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetTextWidth(hint.Content.GetText(), hint.FontSize);
        }

        public float GetTextWidth(string? text, float fontSize, HintAlignment align = HintAlignment.Center)
        {
            IReadOnlyList<LineInfo> lineInfos = GetLineInfos(text, fontSize, align);

            float max = 0f;
            foreach (LineInfo line in lineInfos)
            {
                if (line.Width > max)
                    max = line.Width;
            }

            return max;
        }

        public float GetTextHeight(AbstractHint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetTextHeight(hint.Content.GetText(), hint.FontSize, hint.LineHeight);
        }

        public float GetTextHeight(string? text, float fontSize, float lineHeight)
        {
            if (fontSize < 0)
                throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be greater than zero.");

            IReadOnlyList<LineInfo> lineInfos = GetLineInfos(text, fontSize);

            float height = 0f;
            foreach (LineInfo line in lineInfos)
            {
                height += line.Height + lineHeight;
            }

            return height > 0 ? height - lineHeight : 0f; // Remove the line height of the last line
        }

        public LineInfo[] GetLineInfos(string? text, float fontSize, HintAlignment align = HintAlignment.Center)
        {
            RichTextParser parser = richTextParserPool.Rent();
            settingTemplate.DefaultStyle.CharStyle.FontSize = fontSize;
            settingTemplate.DefaultStyle.LineStyle.Alignment = align;
            RichTextParserResult result = parser.ParseText(text, settingTemplate);
            richTextParserPool.Return(parser);

            return result.LineInfos;
        }

        public float GetYCoordinate(float vOffset)
        {
            return 700 - vOffset;
        }

        public float GetVOffset(float yCoordinate)
        {
            return 700 - yCoordinate;
        }
    }
}
