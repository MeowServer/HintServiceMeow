namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the font-weight rich text tag <c>&lt;font-weight&gt;</c>.
    /// Sets the typographic weight (thickness) of the enclosed text using numeric CSS-style weight values.
    /// Common values are <c>400</c> (normal) and <c>700</c> (bold); custom values require a variable font.
    /// Example: <c>&lt;font-weight="700"&gt;text&lt;/font-weight&gt;</c>
    /// </summary>
    public sealed class FontWeightTag : RichTag
    {
        private readonly string _weight;

        private FontWeightTag(string weight)
        {
            _weight = weight;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<font-weight=\"{_weight}\">";

        /// <inheritdoc/>
        public override string CloseTag => "</font-weight>";

        /// <inheritdoc/>
        internal override int Priority => 250;

        // ── Predefined weights ────────────────────────────────────────────────

        /// <summary>
        /// Normal weight (400). Syntax: <c>&lt;font-weight="400"&gt;text&lt;/font-weight&gt;</c>
        /// </summary>
        public static readonly FontWeightTag Normal = new FontWeightTag("400");

        /// <summary>
        /// Bold weight (700). Syntax: <c>&lt;font-weight="700"&gt;text&lt;/font-weight&gt;</c>
        /// </summary>
        public static readonly FontWeightTag Bold = new FontWeightTag("700");

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="FontWeightTag"/> with a custom numeric weight value.
        /// </summary>
        /// <param name="weight">
        /// A numeric CSS font-weight string (e.g., <c>100</c>, <c>400</c>, <c>700</c>, <c>900</c>).
        /// Values other than <c>400</c> and <c>700</c> require a variable font that supports those weights.
        /// Syntax result: <c>&lt;font-weight="weight"&gt;</c>
        /// </param>
        public static FontWeightTag Get(string weight)
        {
            return new FontWeightTag(weight);
        }
    }
}
