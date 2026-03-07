namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the monospace-spacing rich text tag <c>&lt;mspace&gt;</c>.
    /// Forces each character to occupy the same fixed width, simulating monospace rendering.
    /// Accepts font unit values.
    /// Example: <c>&lt;mspace=1em&gt;text&lt;/mspace&gt;</c>.
    /// Example (pixels): <c>&lt;mspace=10&gt;text&lt;/mspace&gt;</c>.
    /// </summary>
    public sealed class MSpaceTag : RichTag
    {
        private readonly string value;

        private MSpaceTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<mspace={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</mspace>";

        /// <inheritdoc/>
        internal override int Priority => 300;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates an <see cref="MSpaceTag"/> with a custom character width.
        /// </summary>
        /// <param name="value">
        /// The fixed character width in font units (e.g., <c>1em</c>) or pixels (e.g., <c>10</c>).
        /// All characters in the enclosed text will be rendered at this width.
        /// Syntax result: <c>&lt;mspace=value&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="MSpaceTag"/> with the specified character width.</returns>
        public static MSpaceTag Get(string value)
        {
            return new MSpaceTag(value);
        }
    }
}
