namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the indent rich text tag <c>&lt;indent&gt;</c>.
    /// Sets the horizontal start position of the enclosed text, indenting it from the left margin.
    /// Supports pixel values, font units, and percentages.
    /// Example: <c>&lt;indent=20%&gt;text&lt;/indent&gt;</c>
    /// Example (pixels): <c>&lt;indent=15&gt;text&lt;/indent&gt;</c>
    /// </summary>
    public sealed class IndentTag : RichTag
    {
        private readonly string _value;

        private IndentTag(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<indent={_value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</indent>";

        /// <inheritdoc/>
        internal override int Priority => 400;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates an <see cref="IndentTag"/> with a custom indent value.
        /// </summary>
        /// <param name="value">
        /// The indent amount in pixels (e.g., <c>15</c>), font units (e.g., <c>1em</c>),
        /// or as a percentage of the text area width (e.g., <c>20%</c>).
        /// Syntax result: <c>&lt;indent=value&gt;</c>
        /// </param>
        public static IndentTag Get(string value)
        {
            return new IndentTag(value);
        }
    }
}
