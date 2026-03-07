namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the italic rich text tag <c>&lt;i&gt;</c>.
    /// Renders the enclosed text in italic style.
    /// Example: <c>&lt;i&gt;text&lt;/i&gt;</c>.
    /// </summary>
    public sealed class ItalicsTag : RichTag
    {
        /// <summary>
        /// Italic text formatting. Syntax: <c>&lt;i&gt;text&lt;/i&gt;</c>.
        /// </summary>
        public static readonly ItalicsTag Italic = new ItalicsTag();

        private ItalicsTag()
        {
        }

        /// <inheritdoc/>
        public override string OpenTag => "<i>";

        /// <inheritdoc/>
        public override string CloseTag => "</i>";

        /// <inheritdoc/>
        internal override int Priority => 100;
    }
}
