namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the line-height rich text tag <c>&lt;line-height&gt;</c>.
    /// Sets the vertical distance between consecutive lines of text.
    /// Supports pixel values, font units, and percentages.
    /// Example: <c>&lt;line-height=150%&gt;text&lt;/line-height&gt;</c>
    /// Example (pixels): <c>&lt;line-height=30&gt;text&lt;/line-height&gt;</c>
    /// </summary>
    public sealed class LineHeightTag : RichTag
    {
        private readonly string _value;

        private LineHeightTag(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<line-height={_value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</line-height>";

        /// <inheritdoc/>
        internal override int Priority => 400;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="LineHeightTag"/> with a custom line-height value.
        /// </summary>
        /// <param name="value">
        /// The line height in pixels (e.g., <c>30</c>), font units (e.g., <c>1.5em</c>),
        /// or as a percentage of the current font size (e.g., <c>150%</c>).
        /// Syntax result: <c>&lt;line-height=value&gt;</c>
        /// </param>
        public static LineHeightTag Get(string value)
        {
            return new LineHeightTag(value);
        }
    }
}
