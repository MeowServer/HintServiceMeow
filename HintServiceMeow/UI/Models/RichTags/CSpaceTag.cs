namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the character-spacing rich text tag <c>&lt;cspace&gt;</c>.
    /// Adjusts the spacing between individual characters in the enclosed text.
    /// Positive values increase spacing; negative values decrease it.
    /// Example: <c>&lt;cspace=2&gt;text&lt;/cspace&gt;</c>
    /// Example (font units): <c>&lt;cspace=0.5em&gt;text&lt;/cspace&gt;</c>
    /// </summary>
    public sealed class CSpaceTag : RichTag
    {
        private readonly string _value;

        private CSpaceTag(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<cspace={_value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</cspace>";

        /// <inheritdoc/>
        internal override int Priority => 300;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="CSpaceTag"/> with a custom character-spacing value.
        /// </summary>
        /// <param name="value">
        /// The spacing value in pixels (e.g., <c>2</c>) or font units (e.g., <c>0.5em</c>).
        /// Use negative values to tighten character spacing.
        /// Syntax result: <c>&lt;cspace=value&gt;</c>
        /// </param>
        public static CSpaceTag Get(string value)
        {
            return new CSpaceTag(value);
        }
    }
}
