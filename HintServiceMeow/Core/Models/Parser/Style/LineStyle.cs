namespace HintServiceMeow.Core.Models.Parser.Style
{
    using HintServiceMeow.Core.Enum;

    /// <summary>
    /// Represents the style settings for a single line.
    /// </summary>
    internal class LineStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineStyle"/> class.
        /// </summary>
        /// <param name="alignment">The horizontal alignment to apply to the line content.</param>
        /// <param name="lineHeight">The height of the line, pixels. Can be null.</param>
        /// <param name="indent">The amount of indentation to apply to the start of the line, in device-independent units.</param>
        /// <param name="marginLeft">The width of the left margin, in device-independent units.</param>
        /// <param name="marginRight">The width of the right margin, in device-independent units.</param>
        /// <param name="maxWidth">The maximum width allowed for the line, in device-independent units.</param>
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

        /// <summary>
        /// Gets the default line style of SCP:SL game.
        /// </summary>
        public static LineStyle Default => new LineStyle(HintAlignment.Center, null, 0, 0, 0, 1440); // Default value based on SCP:SL // TODO: Verify the Max Width

        /// <summary>
        /// Gets or sets the alignment of the line.
        /// </summary>
        public HintAlignment Alignment { get; set; }

        /// <summary>
        /// Gets or sets the line height of the line in pixels.
        /// </summary>
        public float? LineHeight { get; set; }

        /// <summary>
        /// Gets or sets the indent of the line in pixels. Include both LineIndent and Indent.
        /// </summary>
        public float Indent { get; set; }

        /// <summary>
        /// Gets or sets the width of the left margin in pixels.
        /// </summary>
        public float MarginLeft { get; set; }

        /// <summary>
        /// Gets or sets the width of the right margin, in pixels.
        /// </summary>
        public float MarginRight { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowable width for the line.
        /// </summary>
        public float MaxWidth { get; set; }

        /// <summary>
        /// Determines whether the specified object has equal values to the current LineStyle instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current LineStyle instance.</param>
        /// <returns>true if the specified object have same value with this instance.</returns>
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

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = (hash * 31) + Alignment.GetHashCode();
            hash = (hash * 31) + LineHeight.GetHashCode();
            hash = (hash * 31) + Indent.GetHashCode();
            hash = (hash * 31) + MarginLeft.GetHashCode();
            hash = (hash * 31) + MarginRight.GetHashCode();
            hash = (hash * 31) + MaxWidth.GetHashCode();
            return hash;
        }
    }
}
