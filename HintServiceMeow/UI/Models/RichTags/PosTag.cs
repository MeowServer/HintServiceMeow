namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the horizontal-position rich text tag <c>&lt;pos&gt;</c>.
    /// Moves the text cursor to an absolute horizontal position on the current line.
    /// This is a positional tag with no closing counterpart; text following the tag starts at the specified position.
    /// Example: <c>&lt;pos=50%&gt;text</c>
    /// Example (pixels): <c>&lt;pos=200&gt;text</c>
    /// </summary>
    public sealed class PosTag : RichTag
    {
        private readonly string _value;

        private PosTag(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<pos={_value}>";

        /// <inheritdoc/>
        public override string CloseTag => "";

        /// <inheritdoc/>
        internal override int Priority => 300;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="PosTag"/> that moves the cursor to the specified horizontal position.
        /// </summary>
        /// <param name="value">
        /// The horizontal position in pixels (e.g., <c>200</c>), font units (e.g., <c>5em</c>),
        /// or as a percentage of the text area width (e.g., <c>50%</c>).
        /// Syntax result: <c>&lt;pos=value&gt;</c>
        /// </param>
        public static PosTag Get(string value)
        {
            return new PosTag(value);
        }
    }
}
