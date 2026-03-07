namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the line-indent rich text tag <c>&lt;line-indent&gt;</c>.
    /// Indents only the first line of a text block; subsequent lines start at the normal margin.
    /// Supports pixel values, font units, and percentages.
    /// Example: <c>&lt;line-indent=15%&gt;text&lt;/line-indent&gt;</c>
    /// Example (pixels): <c>&lt;line-indent=20&gt;text&lt;/line-indent&gt;</c>
    /// </summary>
    public sealed class LineIndentTag : RichTag
    {
        private readonly string _value;

        private LineIndentTag(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<line-indent={_value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</line-indent>";

        /// <inheritdoc/>
        internal override int Priority => 400;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="LineIndentTag"/> with a custom first-line indent value.
        /// </summary>
        /// <param name="value">
        /// The indent amount in pixels (e.g., <c>20</c>), font units (e.g., <c>1em</c>),
        /// or as a percentage of the text area width (e.g., <c>15%</c>).
        /// Syntax result: <c>&lt;line-indent=value&gt;</c>
        /// </param>
        public static LineIndentTag Get(string value)
        {
            return new LineIndentTag(value);
        }
    }
}
