using HintServiceMeow.Core.Models.Parser.Style;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Models.Parser
{
    internal struct CharInfo
    {
        private float scaledWidth;
        private char character;

        public CharInfo(char c, CharStyle style)
        {
            character = c;
            scaledWidth = FontTool.Instance.GetCharWidth(c, style.FontSize);
            Style = style;
        }

        public char Character => character;

        public CharStyle Style { get; set; }

        public float Width
        {
            get
            {
                if (CustomWidth.HasValue)
                    return CustomWidth.Value;

                return Style.GetWidth(scaledWidth);
            }
        }

        public float Height => Style.GetHeight();

        public float? CustomWidth { get; set; } = null;
    }
}
