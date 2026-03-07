namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the font-size rich text tag <c>&lt;size&gt;</c>.
    /// Sets the font size of the enclosed text. Supports pixel values, font units, and percentages.
    /// Example: <c>&lt;size=24&gt;text&lt;/size&gt;</c>.
    /// Example (percentage): <c>&lt;size=150%&gt;text&lt;/size&gt;</c>.
    /// </summary>
    public sealed class SizeTag : RichTag
    {
        private readonly string value;

        private SizeTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<size={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</size>";

        /// <inheritdoc/>
        internal override int Priority => 250;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="SizeTag"/> with a custom size value.
        /// </summary>
        /// <param name="value">
        /// The size value. Accepts pixel values (e.g., <c>24</c>), font units (e.g., <c>1.5em</c>),
        /// or percentages relative to the default font size (e.g., <c>150%</c>).
        /// Syntax result: <c>&lt;size=value&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="SizeTag"/> with the specified size value.</returns>
        public static SizeTag Get(string value)
        {
            return new SizeTag(value);
        }

        public static SizeTag Get(int pixel)
        {
            return new SizeTag(pixel.ToString());
        }
    }
}
