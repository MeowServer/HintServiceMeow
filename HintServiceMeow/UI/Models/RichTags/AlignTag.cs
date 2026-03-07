namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the alignment rich text tag <c>&lt;align&gt;</c>.
    /// Sets the horizontal alignment of the enclosed text.
    /// Example: <c>&lt;align="center"&gt;text&lt;/align&gt;</c>.
    /// </summary>
    /// <remarks>
    /// This is a finite-set tag. Use the predefined constants:
    /// <see cref="Left"/>, <see cref="Center"/>, <see cref="Right"/>,
    /// <see cref="Justified"/>, <see cref="Flush"/>.
    /// </remarks>
    public sealed class AlignTag : RichTag
    {
        private readonly string value;

        /// <summary>
        /// Aligns text to the left margin. Syntax: <c>&lt;align="left"&gt;text&lt;/align&gt;</c>.
        /// </summary>
        public static readonly AlignTag Left = new AlignTag("left");

        /// <summary>
        /// Centers text between the left and right margins. Syntax: <c>&lt;align="center"&gt;text&lt;/align&gt;</c>.
        /// </summary>
        public static readonly AlignTag Center = new AlignTag("center");

        /// <summary>
        /// Aligns text to the right margin. Syntax: <c>&lt;align="right"&gt;text&lt;/align&gt;</c>.
        /// </summary>
        public static readonly AlignTag Right = new AlignTag("right");

        /// <summary>
        /// Justifies text so both left and right edges are aligned, adding spacing between words.
        /// Syntax: <c>&lt;align="justified"&gt;text&lt;/align&gt;</c>.
        /// </summary>
        public static readonly AlignTag Justified = new AlignTag("justified");

        /// <summary>
        /// Similar to justified, but also justifies the last line of a paragraph.
        /// Syntax: <c>&lt;align="flush"&gt;text&lt;/align&gt;</c>.
        /// </summary>
        public static readonly AlignTag Flush = new AlignTag("flush");

        private AlignTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<align=\"{this.value}\">";

        /// <inheritdoc/>
        public override string CloseTag => "</align>";

        /// <inheritdoc/>
        internal override int Priority => 400;
    }
}
