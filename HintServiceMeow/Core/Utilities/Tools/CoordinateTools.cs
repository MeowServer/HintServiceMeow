using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Pools;
using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to help calculate coordinate for hints
    /// </summary>
    internal static class CoordinateTools
    {
        private const float CanvasHalfWidth = 1200f;

        public static float GetYCoordinate(Hint hint, HintVerticalAlign to)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetYCoordinate(hint, hint.YCoordinateAlign, to);
        }

        public static float GetYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetYCoordinate(hint.YCoordinate, GetTextHeight(hint), from, to);
        }

        public static float GetYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to)
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

        public static float GetXCoordinateWithAlignment(Hint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetXCoordinateWithAlignment(hint, hint.Alignment);
        }

        public static float GetXCoordinateWithAlignment(Hint hint, HintAlignment alignment)
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

        public static float GetTextWidth(AbstractHint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetTextWidth(hint.Content.GetText(), hint.FontSize);
        }

        public static float GetTextWidth(string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));

            IReadOnlyList<LineInfo> lineInfos = GetLineInfos(text, fontSize, align);

            float max = 0f;
            foreach (var line in lineInfos)
                if (line.Width > max) max = line.Width;

            return max;
        }

        public static float GetTextHeight(AbstractHint hint)
        {
            if (hint == null)
                throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

            return GetTextHeight(hint.Content.GetText(), hint.FontSize, hint.LineHeight);
        }

        public static float GetTextHeight(string text, int fontSize, float lineHeight)
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

        public static IReadOnlyList<LineInfo> GetLineInfos(string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            if (text == null)
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));

            return RichTextParserPool.ParseText(text, fontSize, align);
        }
    }
}
