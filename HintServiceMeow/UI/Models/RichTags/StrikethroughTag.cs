namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the strikethrough rich text tag <c>&lt;s&gt;</c>.
    /// Renders a horizontal line through the middle of the enclosed text.
    /// Example: <c>&lt;s&gt;text&lt;/s&gt;</c>
    /// </summary>
    public sealed class StrikethroughTag : RichTag
    {
        private StrikethroughTag() { }

        /// <inheritdoc/>
        public override string OpenTag => "<s>";

        /// <inheritdoc/>
        public override string CloseTag => "</s>";

        /// <inheritdoc/>
        internal override int Priority => 100;

        /// <summary>
        /// Strikethrough text formatting. Syntax: <c>&lt;s&gt;text&lt;/s&gt;</c>
        /// </summary>
        public static readonly StrikethroughTag Strikethrough = new StrikethroughTag();
    }
}
