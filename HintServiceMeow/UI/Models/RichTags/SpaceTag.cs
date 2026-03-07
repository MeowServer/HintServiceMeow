namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the horizontal-space rich text tag <c>&lt;space&gt;</c>.
    /// Inserts a fixed amount of horizontal whitespace at the current cursor position.
    /// This is a self-closing tag with no closing counterpart.
    /// Example: <c>word&lt;space=10&gt;word</c>.
    /// Example (font units): <c>word&lt;space=1em&gt;word</c>.
    /// </summary>
    public sealed class SpaceTag : RichTag
    {
        private readonly string value;

        private SpaceTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<space={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => string.Empty;

        /// <inheritdoc/>
        internal override int Priority => 50;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="SpaceTag"/> that inserts horizontal whitespace of the given width.
        /// </summary>
        /// <param name="value">
        /// The space width in pixels (e.g., <c>10</c>) or font units (e.g., <c>1em</c>).
        /// Syntax result: <c>&lt;space=value&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="SpaceTag"/> with the specified width.</returns>
        public static SpaceTag Get(string value)
        {
            return new SpaceTag(value);
        }

        public static SpaceTag Get(int pixels)
        {
            return new SpaceTag(pixels.ToString());
        }
    }
}
