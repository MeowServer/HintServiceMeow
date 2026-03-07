namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the superscript rich text tag <c>&lt;sup&gt;</c>.
    /// Renders the enclosed text as superscript (smaller, raised above the baseline).
    /// Example: <c>&lt;sup&gt;text&lt;/sup&gt;</c>
    /// </summary>
    public sealed class SuperscriptTag : RichTag
    {
        private SuperscriptTag() { }

        /// <inheritdoc/>
        public override string OpenTag => "<sup>";

        /// <inheritdoc/>
        public override string CloseTag => "</sup>";

        /// <inheritdoc/>
        internal override int Priority => 100;

        /// <summary>
        /// Superscript text formatting. Syntax: <c>&lt;sup&gt;text&lt;/sup&gt;</c>
        /// </summary>
        public static readonly SuperscriptTag Superscript = new SuperscriptTag();
    }
}
