namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the small-caps rich text tag <c>&lt;smallcaps&gt;</c>.
    /// Renders the enclosed text with uppercase letters scaled down to match lowercase height.
    /// Example: <c>&lt;smallcaps&gt;text&lt;/smallcaps&gt;</c>.
    /// </summary>
    public sealed class SmallcapTag : RichTag
    {
        /// <summary>
        /// Small-caps text transformation. Syntax: <c>&lt;smallcaps&gt;text&lt;/smallcaps&gt;</c>.
        /// </summary>
        public static readonly SmallcapTag Smallcaps = new SmallcapTag();

        private SmallcapTag()
        {
        }

        /// <inheritdoc/>
        public override string OpenTag => "<smallcaps>";

        /// <inheritdoc/>
        public override string CloseTag => "</smallcaps>";

        /// <inheritdoc/>
        internal override int Priority => 120;
    }
}
