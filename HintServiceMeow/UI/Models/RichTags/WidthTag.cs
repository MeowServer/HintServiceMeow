namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the text-width rich text tag <c>&lt;width&gt;</c>.
    /// Restricts the wrapping width of the enclosed text to the specified value,
    /// overriding the container's normal text area width.
    /// Supports pixel values and percentages.
    /// Example: <c>&lt;width=60%&gt;text&lt;/width&gt;</c>.
    /// Example (pixels): <c>&lt;width=300&gt;text&lt;/width&gt;</c>.
    /// </summary>
    public sealed class WidthTag : RichTag
    {
        private readonly string value;

        private WidthTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<width={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</width>";

        /// <inheritdoc/>
        internal override int Priority => 400;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="WidthTag"/> with a custom width constraint.
        /// </summary>
        /// <param name="value">
        /// The width in pixels (e.g., <c>300</c>) or as a percentage of the text area width (e.g., <c>60%</c>).
        /// Syntax result: <c>&lt;width=value&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="WidthTag"/> with the specified width constraint.</returns>
        public static WidthTag Get(string value)
        {
            return new WidthTag(value);
        }

        public static WidthTag Get(int pixels)
        {
            return new WidthTag(pixels.ToString());
        }
    }
}
