namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the hyperlink rich text tag <c>&lt;a&gt;</c>.
    /// Marks the enclosed text as a clickable hyperlink pointing to the given URL.
    /// URLs are limited to 256 characters.
    /// Example: <c>&lt;a href="https://example.com"&gt;click here&lt;/a&gt;</c>.
    /// </summary>
    public sealed class HyperlinkTag : RichTag
    {
        private readonly string href;

        private HyperlinkTag(string href)
        {
            this.href = href;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<a href=\"{this.href}\">";

        /// <inheritdoc/>
        public override string CloseTag => "</a>";

        /// <inheritdoc/>
        internal override int Priority => 500;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="HyperlinkTag"/> pointing to the specified URL.
        /// </summary>
        /// <param name="href">
        /// The URL the hyperlink points to (e.g., <c>https://example.com</c>).
        /// Maximum length is 256 characters.
        /// Syntax result: <c>&lt;a href="href"&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="HyperlinkTag"/> pointing to the specified URL.</returns>
        public static HyperlinkTag Get(string href)
        {
            return new HyperlinkTag(href);
        }
    }
}
