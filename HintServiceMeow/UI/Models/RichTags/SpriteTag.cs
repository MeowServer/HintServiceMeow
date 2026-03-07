namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the sprite rich text tag <c>&lt;sprite&gt;</c>.
    /// Embeds an inline sprite image from a TMP Sprite Asset into the text.
    /// This is a self-closing tag; it does not wrap text.
    /// Example (by name): <c>&lt;sprite name="icon_star"&gt;</c>.
    /// Example (by index): <c>&lt;sprite index=0&gt;</c>.
    /// </summary>
    public sealed class SpriteTag : RichTag
    {
        private readonly string attribute;

        private SpriteTag(string attribute)
        {
            this.attribute = attribute;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<sprite {this.attribute}>";

        /// <inheritdoc/>
        public override string CloseTag => string.Empty;

        /// <inheritdoc/>
        internal override int Priority => 50;

        // ── Factory methods ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="SpriteTag"/> that references a sprite by its name in the Sprite Asset.
        /// </summary>
        /// <param name="spriteName">
        /// The exact name of the sprite within the TMP Sprite Asset (e.g., <c>icon_star</c>).
        /// Syntax result: <c>&lt;sprite name="spriteName"&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="SpriteTag"/> referencing the named sprite.</returns>
        public static SpriteTag Get(string spriteName)
        {
            return new SpriteTag($"name=\"{spriteName}\"");
        }

        /// <summary>
        /// Creates a <see cref="SpriteTag"/> that references a sprite by its zero-based index in the Sprite Asset.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the sprite within the TMP Sprite Asset (e.g., <c>0</c>).
        /// Syntax result: <c>&lt;sprite index=index&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="SpriteTag"/> referencing the sprite at the specified index.</returns>
        public static SpriteTag GetByIndex(int index)
        {
            return new SpriteTag($"index={index}");
        }
    }
}
