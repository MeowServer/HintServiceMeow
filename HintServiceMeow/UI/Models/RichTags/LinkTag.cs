namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the link rich text tag <c>&lt;link&gt;</c>.
    /// Associates a custom string ID with the enclosed text, which can be retrieved at runtime
    /// to implement custom click handling (e.g., opening menus, triggering events).
    /// IDs are limited to 256 characters.
    /// Example: <c>&lt;link="item_42"&gt;click me&lt;/link&gt;</c>.
    /// </summary>
    public sealed class LinkTag : RichTag
    {
        private readonly string id;

        private LinkTag(string id)
        {
            this.id = id;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<link=\"{this.id}\">";

        /// <inheritdoc/>
        public override string CloseTag => "</link>";

        /// <inheritdoc/>
        internal override int Priority => 500;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="LinkTag"/> with a custom link identifier.
        /// </summary>
        /// <param name="id">
        /// A custom string identifier that will be returned by TextMeshPro's link-detection API
        /// when the user interacts with the marked text (e.g., <c>item_42</c>).
        /// Maximum length is 256 characters.
        /// Syntax result: <c>&lt;link="id"&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="LinkTag"/> with the specified link identifier.</returns>
        public static LinkTag Get(string id)
        {
            return new LinkTag(id);
        }
    }
}
