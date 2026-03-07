namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the lowercase rich text tag <c>&lt;lowercase&gt;</c>.
    /// Renders the enclosed text in all lowercase letters without altering the source string.
    /// Example: <c>&lt;lowercase&gt;text&lt;/lowercase&gt;</c>
    /// </summary>
    public sealed class LowercaseTag : RichTag
    {
        private LowercaseTag() { }

        /// <inheritdoc/>
        public override string OpenTag => "<lowercase>";

        /// <inheritdoc/>
        public override string CloseTag => "</lowercase>";

        /// <inheritdoc/>
        internal override int Priority => 120;

        /// <summary>
        /// Lowercase text transformation. Syntax: <c>&lt;lowercase&gt;text&lt;/lowercase&gt;</c>
        /// </summary>
        public static readonly LowercaseTag Lowercase = new LowercaseTag();
    }
}
