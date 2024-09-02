
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    internal class CoordinateTools
    {
        //return 700 - actualYCoordinate;

        public static float GetActualYCoordinate(Hint hint, HintVerticalAlign to)
        {
            return GetActualYCoordinate(hint, hint.YCoordinateAlign, to);
        }

        //Get the Y coordinate without vertical alignment's offset
        public static float GetActualYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to)
        {
            return GetActualYCoordinate(hint.YCoordinate, GetTextHeight(hint), from, to);
        }

        public static float GetActualYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to)
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

        public static float GetXCoordinateWithAlignment(Hint hint)
        {
            return GetXCoordinateWithAlignment(hint, hint.Alignment);
        }

        public static float GetXCoordinateWithAlignment(Hint hint, HintAlignment alignment)
        {
            float alignOffset;

            switch (alignment)
            {
                case HintAlignment.Left:
                    alignOffset = -1200 + GetTextWidth(hint) / 2;
                    break;
                case HintAlignment.Right:
                    alignOffset = 1200 - GetTextWidth(hint) / 2;
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

        public static float GetTextWidth(AbstractHint hint)
        {
            return TextTool.GetTextWidth(hint.Content.GetText(), hint.FontSize);
        }

        public static float GetTextWidth(string text, int fontSize)
        {
            return TextTool.GetTextWidth(text, fontSize);
        }

        public static float GetTextHeight(AbstractHint hint)
        {
            return TextTool.GetTextHeight(hint.Content.GetText(), hint.FontSize, hint.LineHeight);
        }

        public static float GetTextHeight(string text, int fontSize, float lineHeight)
        {
            return TextTool.GetTextHeight(text, fontSize, lineHeight);
        }
    }
}
