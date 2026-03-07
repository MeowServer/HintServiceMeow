namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the vertical-offset rich text tag <c>&lt;voffset&gt;</c>.
    /// Shifts the enclosed text up or down relative to the baseline.
    /// Positive values move text upward; negative values move it downward.
    /// Example: <c>&lt;voffset=2em&gt;text&lt;/voffset&gt;</c>.
    /// Example (pixels): <c>&lt;voffset=-4&gt;text&lt;/voffset&gt;</c>.
    /// </summary>
    public sealed class VOffsetTag : RichTag
    {
        private readonly string value;

        private VOffsetTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<voffset={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</voffset>";

        /// <inheritdoc/>
        internal override int Priority => 300;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="VOffsetTag"/> with a custom vertical offset.
        /// </summary>
        /// <param name="value">
        /// The vertical offset in pixels (e.g., <c>4</c>) or font units (e.g., <c>0.5em</c>).
        /// Positive values raise the text above the baseline; negative values lower it below.
        /// Syntax result: <c>&lt;voffset=value&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="VOffsetTag"/> with the specified vertical offset.</returns>
        public static VOffsetTag Get(string value)
        {
            return new VOffsetTag(value);
        }

        public static VOffsetTag Get(int pixels)
        {
            return new VOffsetTag(pixels.ToString());
        }
    }
}
