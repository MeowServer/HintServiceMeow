namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the alpha (opacity) rich text tag <c>&lt;alpha&gt;</c>.
    /// Adjusts the transparency of the enclosed text without changing its color.
    /// Example: <c>&lt;alpha=#80&gt;text&lt;/alpha&gt;</c>.
    /// </summary>
    public sealed class AlphaTag : RichTag
    {
        // ── Predefined opacity levels ─────────────────────────────────────────

        /// <summary>
        /// Fully transparent (invisible). Syntax: <c>&lt;alpha=#00&gt;text&lt;/alpha&gt;</c>.
        /// </summary>
        public static readonly AlphaTag Transparent = new AlphaTag("00");

        /// <summary>
        /// Semi-transparent (~50% opacity). Syntax: <c>&lt;alpha=#80&gt;text&lt;/alpha&gt;</c>.
        /// </summary>
        public static readonly AlphaTag SemiTransparent = new AlphaTag("80");

        /// <summary>
        /// Fully opaque (100% opacity). Syntax: <c>&lt;alpha=#FF&gt;text&lt;/alpha&gt;</c>.
        /// </summary>
        public static readonly AlphaTag Opaque = new AlphaTag("FF");

        private readonly string hex;

        private AlphaTag(string hex)
        {
            this.hex = hex;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<alpha=#{this.hex}>";

        /// <inheritdoc/>
        public override string CloseTag => "</alpha>";

        /// <inheritdoc/>
        internal override int Priority => 200;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates an <see cref="AlphaTag"/> with a custom alpha value.
        /// </summary>
        /// <param name="hex">
        /// A two-character uppercase hexadecimal string representing the alpha level,
        /// ranging from <c>00</c> (transparent) to <c>FF</c> (opaque). Do not include the <c>#</c> prefix.
        /// Syntax result: <c>&lt;alpha=#HH&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="AlphaTag"/> with the specified alpha level.</returns>
        public static AlphaTag Get(string hex)
        {
            return new AlphaTag(hex);
        }

        /// <summary>
        /// Creates an <see cref="AlphaTag"/> with a custom alpha value.
        /// </summary>
        /// <param name="alpha">A byte representing the alpha level.</param>
        /// <returns>An AlphaTag instance with specified alpha value.</returns>
        public static AlphaTag Get(byte alpha)
        {
            string hex = alpha.ToString("X2");
            return new AlphaTag(hex);
        }
    }
}
