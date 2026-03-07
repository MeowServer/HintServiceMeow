namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the margin rich text tag <c>&lt;margin&gt;</c>.
    /// Sets both the left and right margins of the text block simultaneously.
    /// Supports pixel values, font units, and percentages.
    /// Example: <c>&lt;margin=10%&gt;text&lt;/margin&gt;</c>.
    /// Example (pixels): <c>&lt;margin=20&gt;text&lt;/margin&gt;</c>.
    /// </summary>
    public sealed class MarginTag : RichTag
    {
        private readonly string value;

        private MarginTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<margin={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</margin>";

        /// <inheritdoc/>
        internal override int Priority => 400;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="MarginTag"/> with a custom margin value applied to both sides.
        /// </summary>
        /// <param name="value">
        /// The margin amount in pixels (e.g., <c>20</c>), font units (e.g., <c>1em</c>),
        /// or as a percentage of the text area width (e.g., <c>10%</c>).
        /// Syntax result: <c>&lt;margin=value&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="MarginTag"/> with the specified margin value.</returns>
        public static MarginTag Get(string value)
        {
            return new MarginTag(value);
        }
    }
}
