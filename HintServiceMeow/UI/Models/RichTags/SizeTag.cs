namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the font-size rich text tag <c>&lt;size&gt;</c>.
    /// Sets the font size of the enclosed text. Supports pixel values, font units, and percentages.
    /// Example: <c>&lt;size=24&gt;text&lt;/size&gt;</c>
    /// Example (percentage): <c>&lt;size=150%&gt;text&lt;/size&gt;</c>
    /// </summary>
    public sealed class SizeTag : RichTag
    {
        private readonly string _value;

        private SizeTag(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<size={_value}>";

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
        /// Syntax result: <c>&lt;size=value&gt;</c>
        /// </param>
        public static SizeTag Get(string value)
        {
            return new SizeTag(value);
        }
    }
}
