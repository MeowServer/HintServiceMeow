using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using System.Collections.Generic;
using System.Linq;
using PluginAPI.Core;
using NorthwoodLib;
using HintServiceMeow.Core.Utilities.Pools;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to help calculate coordinate for hints
    /// </summary>
    internal class CoordinateTools
    {
        public static float GetYCoordinate(Hint hint, HintVerticalAlign to)
        {
            return GetYCoordinate(hint, hint.YCoordinateAlign, to);
        }

        public static float GetYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to)
        {
            return GetYCoordinate(hint.YCoordinate, GetTextHeight(hint), from, to);
        }

        public static float GetYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to)
        {
            float offset = 0;

            offset += from switch
            {
                HintVerticalAlign.Top => textHeight,
                HintVerticalAlign.Middle => textHeight / 2,
                _ => 0
            };

            offset -= to switch
            {
                HintVerticalAlign.Top => textHeight,
                HintVerticalAlign.Middle => textHeight / 2,
                _ => 0
            };

            return rawYCoordinate + offset;
        }

        /// <summary>
        /// Get the X coordinate of the hint with alignment's offset
        /// </summary>
        public static float GetXCoordinateWithAlignment(Hint hint)
        {
            return GetXCoordinateWithAlignment(hint, hint.Alignment);
        }

        /// <summary>
        /// Get the X coordinate of the hint with alignment's offset
        /// </summary>
        public static float GetXCoordinateWithAlignment(Hint hint, HintAlignment alignment)
        {
            float alignOffset = alignment switch
            {
                HintAlignment.Left => -1200 + GetTextWidth(hint) / 2,
                HintAlignment.Right => 1200 - GetTextWidth(hint) / 2,
                _ => 0
            };

            return hint.XCoordinate + alignOffset;
        }

        public static float GetTextWidth(AbstractHint hint)
        {
            return GetTextWidth(hint.Content.GetText(), hint.FontSize);
        }

        public static float GetTextWidth(string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            var lineInfos = GetLineInfos(text, fontSize, align);

            return lineInfos.Max(x => x.Width);
        }

        public static float GetTextHeight(AbstractHint hint)
        {
            return GetTextHeight(hint.Content.GetText(), hint.FontSize, hint.LineHeight);
        }

        public static float GetTextHeight(string text, int fontSize, float lineHeight)
        {
            var lineInfos = GetLineInfos(text, fontSize);

            return lineInfos.Sum(x => x.Height) + lineInfos.Count * lineHeight;
        }

        public static IReadOnlyCollection<LineInfo> GetLineInfos(Hint hint)
        {
            return GetLineInfos(hint.Content.GetText(), hint.FontSize, hint.Alignment);
        }

        public static IReadOnlyCollection<LineInfo> GetLineInfos(string text, int fontSize, HintAlignment align = HintAlignment.Center)
        {
            return RichTextParserPool.ParseText(text, fontSize, align);
        }
    }
}
