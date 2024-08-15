
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
        public static float GetVOffset(Hint hint)
        {
            return GetVOffset(hint, hint.YCoordinateAlign);
        }

        //The the Pos value represented by the y coordinate of the hint
        public static float GetVOffset(Hint hint, HintVerticalAlign align)
        {
            float sizeOffset;

            switch (align)
            {
                case HintVerticalAlign.Top:
                    sizeOffset = - GetTextHeight(hint);
                    break;
                case HintVerticalAlign.Middle:
                    sizeOffset = - GetTextHeight(hint) / 2;
                    break;
                case HintVerticalAlign.Bottom:
                    sizeOffset = 0;
                    break;
                default:
                    sizeOffset = 0;
                    break;
            }

            return 700 - hint.YCoordinate + sizeOffset;
        }

        //Get the Y coordiate without vertical alignment
        public static float GetActualYCoordinate(Hint hint, HintVerticalAlign align)
        {
            float sizeOffset;

            switch (align)
            {
                case HintVerticalAlign.Top:
                    sizeOffset = -GetTextHeight(hint);
                    break;
                case HintVerticalAlign.Middle:
                    sizeOffset = -GetTextHeight(hint) / 2;
                    break;
                case HintVerticalAlign.Bottom:
                    sizeOffset = 0;
                    break;
                default:
                    sizeOffset = 0;
                    break;
            }

            return hint.YCoordinate + sizeOffset;
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
                    alignOffset = -1200 + FontTool.GetTextWidth(hint) / 2;
                    break;
                case HintAlignment.Right:
                    alignOffset = 1200 - FontTool.GetTextWidth(hint) / 2;
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
            return FontTool.GetTextWidth(hint);
        }

        public static float GetTextHeight(AbstractHint hint)
        {
            var content = hint.Content.GetText();

            if(string.IsNullOrEmpty(content))
                return 0;

            var height = content.Split('\n').Length * hint.FontSize;

            return height;
        }
    }
}
