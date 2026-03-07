namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the alpha (opacity) rich text tag <c>&lt;alpha&gt;</c>.
    /// Adjusts the transparency of the enclosed text without changing its color.
    /// Example: <c>&lt;alpha=#80&gt;text&lt;/alpha&gt;</c>
    /// </summary>
    public sealed class AlphaTag : RichTag
    {
        private readonly string _hex;

        private AlphaTag(string hex)
        {
            _hex = hex;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<alpha=#{_hex}>";

        /// <inheritdoc/>
        public override string CloseTag => "</alpha>";

        /// <inheritdoc/>
        internal override int Priority => 200;

        // ── Predefined opacity levels ─────────────────────────────────────────

        /// <summary>
        /// Fully transparent (invisible). Syntax: <c>&lt;alpha=#00&gt;text&lt;/alpha&gt;</c>
        /// </summary>
        public static readonly AlphaTag Transparent = new AlphaTag("00");

        /// <summary>
        /// Semi-transparent (~50% opacity). Syntax: <c>&lt;alpha=#80&gt;text&lt;/alpha&gt;</c>
        /// </summary>
        public static readonly AlphaTag SemiTransparent = new AlphaTag("80");

        /// <summary>
        /// Fully opaque (100% opacity). Syntax: <c>&lt;alpha=#FF&gt;text&lt;/alpha&gt;</c>
        /// </summary>
        public static readonly AlphaTag Opaque = new AlphaTag("FF");

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates an <see cref="AlphaTag"/> with a custom alpha value.
        /// </summary>
        /// <param name="hex">
        /// A two-character uppercase hexadecimal string representing the alpha level,
        /// ranging from <c>00</c> (transparent) to <c>FF</c> (opaque). Do not include the <c>#</c> prefix.
        /// Syntax result: <c>&lt;alpha=#HH&gt;</c>
        /// </param>
        public static AlphaTag Get(string hex)
        {
            return new AlphaTag(hex);
        }
    }
}
