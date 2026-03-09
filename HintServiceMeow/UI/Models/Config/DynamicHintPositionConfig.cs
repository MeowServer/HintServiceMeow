namespace HintServiceMeow.UI.Models.Config
{
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// A lightweight, position-only configuration class for a <see cref="DynamicHint"/>.
    /// Contains only the boundary, target, and margin properties that control where a dynamic
    /// hint is placed on screen, without any content or display properties.
    /// <para>
    /// Use this class when you only need to adjust the layout region of an existing
    /// <see cref="DynamicHint"/> without touching its text, font, priority, or strategy.
    /// Intentionally does <b>not</b> inherit from <see cref="AbstractHintConfig"/> to keep
    /// the surface area small.
    /// </para>
    /// <para>
    /// All properties are nullable. Only non-null values are applied by <see cref="ApplyTo"/>.
    /// </para>
    /// </summary>
    public class DynamicHintPositionConfig
    {
        // ── Boundaries ────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the top boundary of the hint's allowed display area.
        /// Maps to <see cref="DynamicHint.TopBoundary"/>.
        /// </summary>
        public float? TopBoundary { get; set; }

        /// <summary>
        /// Gets or sets the bottom boundary of the hint's allowed display area.
        /// Maps to <see cref="DynamicHint.BottomBoundary"/>.
        /// </summary>
        public float? BottomBoundary { get; set; }

        /// <summary>
        /// Gets or sets the left boundary of the hint's allowed display area.
        /// Should be no less than -1200.
        /// Maps to <see cref="DynamicHint.LeftBoundary"/>.
        /// </summary>
        public float? LeftBoundary { get; set; }

        /// <summary>
        /// Gets or sets the right boundary of the hint's allowed display area.
        /// Should be no greater than 1200.
        /// Maps to <see cref="DynamicHint.RightBoundary"/>.
        /// </summary>
        public float? RightBoundary { get; set; }

        // ── Target coordinates ────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the preferred X coordinate that the hint will try to reach.
        /// Maps to <see cref="DynamicHint.TargetX"/>.
        /// </summary>
        public float? TargetX { get; set; }

        /// <summary>
        /// Gets or sets the preferred Y coordinate that the hint will try to reach.
        /// Maps to <see cref="DynamicHint.TargetY"/>.
        /// </summary>
        public float? TargetY { get; set; }

        // ── Margins ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the spacing in pixels between this hint and any hint placed above it.
        /// Maps to <see cref="DynamicHint.TopMargin"/>.
        /// </summary>
        public float? TopMargin { get; set; }

        /// <summary>
        /// Gets or sets the spacing in pixels between this hint and any hint placed below it.
        /// Maps to <see cref="DynamicHint.BottomMargin"/>.
        /// </summary>
        public float? BottomMargin { get; set; }

        /// <summary>
        /// Gets or sets the left spacing in pixels applied during horizontal positioning.
        /// Maps to <see cref="DynamicHint.LeftMargin"/>.
        /// </summary>
        public float? LeftMargin { get; set; }

        /// <summary>
        /// Gets or sets the right spacing in pixels applied during horizontal positioning.
        /// Maps to <see cref="DynamicHint.RightMargin"/>.
        /// </summary>
        public float? RightMargin { get; set; }

        // ── Methods ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the non-null positional and layout properties of this config to the given
        /// <paramref name="hint"/>. Existing hint values are preserved for any property
        /// that remains null.
        /// </summary>
        /// <param name="hint">The target <see cref="DynamicHint"/> to reposition.</param>
        public void ApplyTo(DynamicHint hint)
        {
            if (TopBoundary.HasValue)
                hint.TopBoundary = TopBoundary.Value;

            if (BottomBoundary.HasValue)
                hint.BottomBoundary = BottomBoundary.Value;

            if (LeftBoundary.HasValue)
                hint.LeftBoundary = LeftBoundary.Value;

            if (RightBoundary.HasValue)
                hint.RightBoundary = RightBoundary.Value;

            if (TargetX.HasValue)
                hint.TargetX = TargetX.Value;

            if (TargetY.HasValue)
                hint.TargetY = TargetY.Value;

            if (TopMargin.HasValue)
                hint.TopMargin = TopMargin.Value;

            if (BottomMargin.HasValue)
                hint.BottomMargin = BottomMargin.Value;

            if (LeftMargin.HasValue)
                hint.LeftMargin = LeftMargin.Value;

            if (RightMargin.HasValue)
                hint.RightMargin = RightMargin.Value;
        }
    }
}
