namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the highlight mark rich text tag <c>&lt;mark&gt;</c>.
    /// Draws a colored highlight behind the enclosed text. The color value must include an alpha channel.
    /// Example: <c>&lt;mark=#FFFF00AA&gt;text&lt;/mark&gt;</c>.
    /// </summary>
    public sealed class MarkTag : RichTag
    {
        private readonly string value;

        // ── Predefined highlight colors ───────────────────────────────────────

        /// <summary>
        /// Semi-transparent yellow highlight. Syntax: <c>&lt;mark=#FFFF00AA&gt;text&lt;/mark&gt;</c>.
        /// </summary>
        public static readonly MarkTag Yellow = new MarkTag("#FFFF00AA");

        /// <summary>
        /// Semi-transparent cyan highlight. Syntax: <c>&lt;mark=#00FFFFAA&gt;text&lt;/mark&gt;</c>.
        /// </summary>
        public static readonly MarkTag Cyan = new MarkTag("#00FFFFAA");

        /// <summary>
        /// Semi-transparent red highlight. Syntax: <c>&lt;mark=#FF0000AA&gt;text&lt;/mark&gt;</c>.
        /// </summary>
        public static readonly MarkTag Red = new MarkTag("#FF0000AA");

        /// <summary>
        /// Semi-transparent green highlight. Syntax: <c>&lt;mark=#00FF00AA&gt;text&lt;/mark&gt;</c>.
        /// </summary>
        public static readonly MarkTag Green = new MarkTag("#00FF00AA");

        private MarkTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<mark={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</mark>";

        /// <inheritdoc/>
        internal override int Priority => 200;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="MarkTag"/> with a custom highlight color.
        /// </summary>
        /// <param name="hexWithAlpha">
        /// An 8-character hex color string with a leading <c>#</c> in RRGGBBAA format,
        /// e.g., <c>#FFFF00AA</c>. The alpha channel (<c>AA</c>) controls highlight transparency;
        /// use a value less than <c>FF</c> so the underlying text remains visible.
        /// Syntax result: <c>&lt;mark=#RRGGBBAA&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="MarkTag"/> with the specified highlight color.</returns>
        public static MarkTag Get(string hexWithAlpha)
        {
            return new MarkTag(hexWithAlpha);
        }
    }
}
