namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the font rich text tag <c>&lt;font&gt;</c>.
    /// Switches the typeface used for the enclosed text to the named Font Asset.
    /// The font name must exactly match a Font Asset that has been loaded by TextMeshPro.
    /// Example: <c>&lt;font="LiberationSans SDF"&gt;text&lt;/font&gt;</c>.
    /// </summary>
    public sealed class FontTag : RichTag
    {
        private readonly string fontName;

        private FontTag(string fontName)
        {
            this.fontName = fontName;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<font=\"{this.fontName}\">";

        /// <inheritdoc/>
        public override string CloseTag => "</font>";

        /// <inheritdoc/>
        internal override int Priority => 250;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="FontTag"/> for the specified Font Asset.
        /// </summary>
        /// <param name="fontAssetName">
        /// The exact name of the TextMeshPro Font Asset to use (e.g., <c>LiberationSans SDF</c>).
        /// The name is case-sensitive and must match the asset registered in the project.
        /// Syntax result: <c>&lt;font="fontAssetName"&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="FontTag"/> for the specified font asset.</returns>
        public static FontTag Get(string fontAssetName)
        {
            return new FontTag(fontAssetName);
        }
    }
}
