using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using System;
using System.Collections.Generic;
using HintServiceMeow.Core.Utilities.Pools;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to help calculate coordinate for hints
    /// </summary>
    internal class CoordinateTools : ICoordinateTools
    {
        private const float CanvasHalfWidth = 1200f;

        private IPool<RichTextParser> _richTextParserPool { get; }

        public CoordinateTools(IPool<RichTextParser> richTextParserPool = null)
        {
            _richTextParserPool = richTextParserPool ?? RichTextParserPool.Instance;
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
            }

            switch (to)
            {
                case HintVerticalAlign.Top:
                    offset -= textHeight;
                    break;
                case HintVerticalAlign.Middle:
                    offset -= textHeight / 2;
                    break;
            }

            return rawYCoordinate + offset;
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
            float alignOffset;
            switch (alignment)
            {
                case HintAlignment.Left:
                    alignOffset = -CanvasHalfWidth + width / 2;
                    break;
                case HintAlignment.Right:
                    alignOffset = CanvasHalfWidth - width / 2;
                    break;
                default:
                    alignOffset = 0;
                    break;
            }

            return hint.XCoordinate + alignOffset;
        }

        public float GetTextWidth(AbstractHint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetTextWidth(hint.Content.GetText(), hint.FontSize);
        }

        public float GetTextWidth(string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            IReadOnlyList<LineInfo> lineInfos = GetLineInfos(text, fontSize, align);

            float max = 0f;
            foreach (var line in lineInfos)
                if (line.Width > max) max = line.Width;

            return max;
        }

        public float GetTextHeight(AbstractHint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetTextHeight(hint.Content.GetText(), hint.FontSize, hint.LineHeight);
        }

        public float GetTextHeight(string text, int fontSize, float lineHeight)
        {
            if (fontSize < 0)
                throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be greater than zero.");

            if (lineHeight < 0)
                throw new ArgumentOutOfRangeException(nameof(lineHeight), "Line height cannot be negative.");

            IReadOnlyList<LineInfo> lineInfos = GetLineInfos(text, fontSize);

            float height = 0f;
            foreach (var line in lineInfos)
            {
                height += line.Height + lineHeight;
            }

            return height > 0 ? height - lineHeight : 0f; // Remove the line height of the last line
        }

        public IReadOnlyList<LineInfo> GetLineInfos(string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            RichTextParser parser = _richTextParserPool.Rent();
            IReadOnlyList<LineInfo> result = parser.ParseText(text, fontSize, align);
            _richTextParserPool.Return(parser);

            return result;
        }
    }
}
