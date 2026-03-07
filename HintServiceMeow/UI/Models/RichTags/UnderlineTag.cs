namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the underline rich text tag <c>&lt;u&gt;</c>.
    /// Renders a line beneath the enclosed text.
    /// Example: <c>&lt;u&gt;text&lt;/u&gt;</c>.
    /// </summary>
    public sealed class UnderlineTag : RichTag
    {
        /// <summary>
        /// Underline text formatting. Syntax: <c>&lt;u&gt;text&lt;/u&gt;</c>.
        /// </summary>
        public static readonly UnderlineTag Underline = new UnderlineTag();

        private UnderlineTag()
        {
        }

        /// <inheritdoc/>
        public override string OpenTag => "<u>";

        /// <inheritdoc/>
        public override string CloseTag => "</u>";

        /// <inheritdoc/>
        internal override int Priority => 100;
    }
}
