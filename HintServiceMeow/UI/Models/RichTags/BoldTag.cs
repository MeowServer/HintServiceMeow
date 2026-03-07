namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the bold rich text tag <c>&lt;b&gt;</c>.
    /// Renders the enclosed text in bold weight.
    /// Example: <c>&lt;b&gt;text&lt;/b&gt;</c>
    /// </summary>
    public sealed class BoldTag : RichTag
    {
        private BoldTag() { }

        /// <inheritdoc/>
        public override string OpenTag => "<b>";

        /// <inheritdoc/>
        public override string CloseTag => "</b>";

        /// <inheritdoc/>
        internal override int Priority => 100;

        /// <summary>
        /// Bold text formatting. Syntax: <c>&lt;b&gt;text&lt;/b&gt;</c>
        /// </summary>
        public static readonly BoldTag Bold = new BoldTag();
    }
}
