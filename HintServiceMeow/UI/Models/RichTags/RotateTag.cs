namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the character-rotation rich text tag <c>&lt;rotate&gt;</c>.
    /// Rotates each character in the enclosed text by the specified number of degrees.
    /// Positive values rotate counter-clockwise; negative values rotate clockwise.
    /// Example: <c>&lt;rotate=45&gt;text&lt;/rotate&gt;</c>
    /// Example (clockwise): <c>&lt;rotate=-90&gt;text&lt;/rotate&gt;</c>
    /// </summary>
    public sealed class RotateTag : RichTag
    {
        private readonly string _value;

        private RotateTag(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<rotate={_value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</rotate>";

        /// <inheritdoc/>
        internal override int Priority => 300;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="RotateTag"/> with a custom rotation angle.
        /// </summary>
        /// <param name="degrees">
        /// The rotation angle as a numeric string (e.g., <c>45</c>, <c>-90</c>, <c>180</c>).
        /// Positive values rotate characters counter-clockwise; negative values rotate clockwise.
        /// Syntax result: <c>&lt;rotate=degrees&gt;</c>
        /// </param>
        public static RotateTag Get(string degrees)
        {
            return new RotateTag(degrees);
        }
    }
}
