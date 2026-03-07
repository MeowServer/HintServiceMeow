namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the line-break rich text tag <c>&lt;br&gt;</c>.
    /// Inserts a hard line break at the point it is placed.
    /// This is a self-closing tag with no closing counterpart.
    /// Example: <c>line one&lt;br&gt;line two</c>
    /// </summary>
    public sealed class BreakTag : RichTag
    {
        private BreakTag() { }

        /// <inheritdoc/>
        public override string OpenTag => "<br>";

        /// <inheritdoc/>
        public override string CloseTag => "";

        /// <inheritdoc/>
        internal override int Priority => 50;

        /// <summary>
        /// Inserts a hard line break. When <see cref="RichTag.Apply"/> is called, the break is
        /// prepended before the supplied text. Syntax: <c>&lt;br&gt;</c>
        /// </summary>
        public static readonly BreakTag Break = new BreakTag();
    }
}
