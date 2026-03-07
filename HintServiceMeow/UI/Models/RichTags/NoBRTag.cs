namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the no-break rich text tag <c>&lt;nobr&gt;</c>.
    /// Prevents line breaks within the enclosed text, keeping it on a single line.
    /// Example: <c>&lt;nobr&gt;text&lt;/nobr&gt;</c>.
    /// </summary>
    public sealed class NoBRTag : RichTag
    {
        /// <summary>
        /// No-break wrapper that prevents automatic line breaking. Syntax: <c>&lt;nobr&gt;text&lt;/nobr&gt;</c>.
        /// </summary>
        public static readonly NoBRTag NoBr = new NoBRTag();

        private NoBRTag()
        {
        }

        /// <inheritdoc/>
        public override string OpenTag => "<nobr>";

        /// <inheritdoc/>
        public override string CloseTag => "</nobr>";

        /// <inheritdoc/>
        internal override int Priority => 150;
    }
}
