using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;

using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to help calculate coordinate for hints
    /// </summary>
    internal class CoordinateTools
    {
        public static float GetYCoordinate(RichTextParser parser, Hint hint, HintVerticalAlign to)
        {
            return GetYCoordinate(parser, hint, hint.YCoordinateAlign, to);
        }

        public static float GetYCoordinate(RichTextParser parser, Hint hint, HintVerticalAlign from, HintVerticalAlign to)
        {
            return GetYCoordinate(hint.YCoordinate, GetTextHeight(parser, hint), from, to);
        }

        public static float GetYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to)
        {
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

        /// <summary>
        /// Get the X coordinate of the hint with alignment's offset
        /// </summary>
        public static float GetXCoordinateWithAlignment(RichTextParser parser, Hint hint)
        {
            return GetXCoordinateWithAlignment(parser, hint, hint.Alignment);
        }

        /// <summary>
        /// Get the X coordinate of the hint with alignment's offset
        /// </summary>
        public static float GetXCoordinateWithAlignment(RichTextParser parser, Hint hint, HintAlignment alignment)
        {
            float alignOffset;

            switch (alignment)
            {
                case HintAlignment.Left:
                    alignOffset = -1200 + GetTextWidth(parser, hint) / 2;
                    break;
                case HintAlignment.Right:
                    alignOffset = 1200 - GetTextWidth(parser, hint) / 2;
                    break;
                case HintAlignment.Center:
                    alignOffset = 0;
                    break;
                default:
                    alignOffset = 0;
                    break;
            }

            return hint.XCoordinate + alignOffset;
        }

        public static float GetTextWidth(RichTextParser parser, AbstractHint hint)
        {
            return GetTextWidth(parser, hint.Content.GetText(), hint.FontSize);
        }

        public static float GetTextWidth(RichTextParser parser, string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            return GetLineInfos(parser, text, fontSize, align).Max(x => x.Width);
        }

        public static float GetTextHeight(RichTextParser parser, AbstractHint hint)
        {
            return GetTextHeight(parser, hint.Content.GetText(), hint.FontSize);
        }

        public static float GetTextHeight(RichTextParser parser, string text, int fontSize)
        {
            return GetLineInfos(parser, text, fontSize).Sum(x => x.Height);
        }

        public static IReadOnlyCollection<LineInfo> GetLineInfos(RichTextParser parser, Hint hint)
        {
            return parser.ParseText(hint.Content.GetText(), hint.FontSize, hint.Alignment);
        }

        public static IReadOnlyCollection<LineInfo> GetLineInfos(RichTextParser parser, string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            return parser.ParseText(text, fontSize, align);
        }
    }
}
