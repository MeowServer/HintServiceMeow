namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the all-caps rich text tag <c>&lt;allcaps&gt;</c>.
    /// Renders the enclosed text in all uppercase letters without altering the source string.
    /// Example: <c>&lt;allcaps&gt;text&lt;/allcaps&gt;</c>
    /// </summary>
    public sealed class AllcapsTag : RichTag
    {
        private AllcapsTag() { }

        /// <inheritdoc/>
        public override string OpenTag => "<allcaps>";

        /// <inheritdoc/>
        public override string CloseTag => "</allcaps>";

        /// <inheritdoc/>
        internal override int Priority => 120;

        /// <summary>
        /// All-caps text transformation. Syntax: <c>&lt;allcaps&gt;text&lt;/allcaps&gt;</c>
        /// </summary>
        public static readonly AllcapsTag Allcaps = new AllcapsTag();
    }
}
