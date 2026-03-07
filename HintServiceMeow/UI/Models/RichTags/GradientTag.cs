namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the gradient rich text tag <c>&lt;gradient&gt;</c>.
    /// Applies a color gradient to the enclosed text using a named Gradient Asset.
    /// The gradient name must exactly match a TMP_ColorGradient asset loaded in the project.
    /// Example: <c>&lt;gradient="Fire"&gt;text&lt;/gradient&gt;</c>
    /// </summary>
    public sealed class GradientTag : RichTag
    {
        private readonly string _gradientName;

        private GradientTag(string gradientName)
        {
            _gradientName = gradientName;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<gradient=\"{_gradientName}\">";

        /// <inheritdoc/>
        public override string CloseTag => "</gradient>";

        /// <inheritdoc/>
        internal override int Priority => 250;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="GradientTag"/> for the specified Gradient Asset.
        /// </summary>
        /// <param name="gradientAssetName">
        /// The exact name of the TMP_ColorGradient asset to apply (e.g., <c>Fire</c>).
        /// The name is case-sensitive and must match the asset registered in the project.
        /// Syntax result: <c>&lt;gradient="gradientAssetName"&gt;</c>
        /// </param>
        public static GradientTag Get(string gradientAssetName)
        {
            return new GradientTag(gradientAssetName);
        }
    }
}
