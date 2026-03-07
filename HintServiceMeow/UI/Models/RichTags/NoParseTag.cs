namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the no-parse rich text tag <c>&lt;noparse&gt;</c>.
    /// Prevents the rich text parser from interpreting any tags within the enclosed content,
    /// causing them to be displayed as literal text.
    /// Example: <c>&lt;noparse&gt;&lt;b&gt;not bold&lt;/b&gt;&lt;/noparse&gt;</c>
    /// </summary>
    public sealed class NoParseTag : RichTag
    {
        private NoParseTag() { }

        /// <inheritdoc/>
        public override string OpenTag => "<noparse>";

        /// <inheritdoc/>
        public override string CloseTag => "</noparse>";

        /// <inheritdoc/>
        internal override int Priority => 150;

        /// <summary>
        /// No-parse wrapper that disables tag interpretation inside. Syntax: <c>&lt;noparse&gt;text&lt;/noparse&gt;</c>
        /// </summary>
        public static readonly NoParseTag NoParse = new NoParseTag();
    }
}
