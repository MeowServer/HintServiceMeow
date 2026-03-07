namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the style rich text tag <c>&lt;style&gt;</c>.
    /// Applies a named TMP Style Sheet entry to the enclosed text, expanding into the
    /// opening and closing markup defined by that style.
    /// Example: <c>&lt;style="Title"&gt;text&lt;/style&gt;</c>
    /// </summary>
    public sealed class StyleTag : RichTag
    {
        private readonly string _styleName;

        private StyleTag(string styleName)
        {
            _styleName = styleName;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<style=\"{_styleName}\">";

        /// <inheritdoc/>
        public override string CloseTag => "</style>";

        /// <inheritdoc/>
        internal override int Priority => 250;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="StyleTag"/> for the specified TMP style name.
        /// </summary>
        /// <param name="styleName">
        /// The name of the style defined in a TMP Style Sheet asset (e.g., <c>Title</c>, <c>Subtitle</c>).
        /// The name is case-sensitive and must match a style entry in the active style sheet.
        /// Syntax result: <c>&lt;style="styleName"&gt;</c>
        /// </param>
        public static StyleTag Get(string styleName)
        {
            return new StyleTag(styleName);
        }
    }
}
