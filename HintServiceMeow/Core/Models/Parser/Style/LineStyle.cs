using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Models.Parser.Style
{
    internal class LineStyle
    {
        public HintAlignment Alignment { get; set; }

        public float? LineHeight { get; set; }

        /// <summary>
        /// Gets the indent of the line. Include both LineIndent and Indent.
        /// </summary>
        public float Indent { get; set; }

        public float MarginLeft { get; set; }

        public float MarginRight { get; set; }

        public float MaxWidth { get; set; }

        public static LineStyle Default => new LineStyle(HintAlignment.Center, null, 0, 0, 0, 1440); // Default value based on SCP:SL // TODO: Verify the Max Width

        public LineStyle(
            HintAlignment alignment,
            float? lineHeight,
            float indent,
            float marginLeft,
            float marginRight,
            float maxWidth)
        {
            Alignment = alignment;
            LineHeight = lineHeight;
            Indent = indent;
            MarginLeft = marginLeft;
            MarginRight = marginRight;
            MaxWidth = maxWidth;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not LineStyle other)
                return false;

            return Alignment == other.Alignment
                && LineHeight == other.LineHeight
                && Indent == other.Indent
                && MarginLeft == other.MarginLeft
                && MarginRight == other.MarginRight
                && MaxWidth == other.MaxWidth;
        }
    }
}
