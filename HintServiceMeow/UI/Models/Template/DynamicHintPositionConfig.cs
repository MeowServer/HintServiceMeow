namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Position-only template for a <see cref="DynamicHint"/>.
    /// Use when you only need to adjust the layout region of an existing dynamic hint without touching any other settings.
    /// Intentionally does not inherit from <see cref="AbstractHintTemplate"/> to keep the surface area minimal.
    /// </summary>
    public class DynamicHintPositionConfig
    {
        // ── Boundaries ────────────────────────────────────────────────────────────

        /// <summary>Gets or sets the top boundary. Maps to <see cref="DynamicHint.TopBoundary"/>.</summary>
        public float? TopBoundary { get; set; }

        /// <summary>Gets or sets the bottom boundary. Maps to <see cref="DynamicHint.BottomBoundary"/>.</summary>
        public float? BottomBoundary { get; set; }

        /// <summary>
        /// Gets or sets the left boundary. Should be no less than -1200.
        /// Maps to <see cref="DynamicHint.LeftBoundary"/>.
        /// </summary>
        public float? LeftBoundary { get; set; }

        /// <summary>
        /// Gets or sets the right boundary. Should be no greater than 1200.
        /// Maps to <see cref="DynamicHint.RightBoundary"/>.
        /// </summary>
        public float? RightBoundary { get; set; }

        // ── Target coordinates ────────────────────────────────────────────────────

        /// <summary>Gets or sets the preferred X coordinate. Maps to <see cref="DynamicHint.TargetX"/>.</summary>
        public float? TargetX { get; set; }

        /// <summary>Gets or sets the preferred Y coordinate. Maps to <see cref="DynamicHint.TargetY"/>.</summary>
        public float? TargetY { get; set; }

        // ── Margins ───────────────────────────────────────────────────────────────

        /// <summary>Gets or sets the spacing above this hint. Maps to <see cref="DynamicHint.TopMargin"/>.</summary>
        public float? TopMargin { get; set; }

        /// <summary>Gets or sets the spacing below this hint. Maps to <see cref="DynamicHint.BottomMargin"/>.</summary>
        public float? BottomMargin { get; set; }

        /// <summary>Gets or sets the left spacing during horizontal positioning. Maps to <see cref="DynamicHint.LeftMargin"/>.</summary>
        public float? LeftMargin { get; set; }

        /// <summary>Gets or sets the right spacing during horizontal positioning. Maps to <see cref="DynamicHint.RightMargin"/>.</summary>
        public float? RightMargin { get; set; }

        // ── Methods ───────────────────────────────────────────────────────────────

        /// <summary>Applies all non-null layout properties to <paramref name="dynamicHint"/>.</summary>
        /// <param name="dynamicHint">The hint to apply these properties to.</param>
        public virtual void Apply(DynamicHint dynamicHint)
        {
            if (TopBoundary.HasValue)
                dynamicHint.TopBoundary = TopBoundary.Value;

            if (BottomBoundary.HasValue)
                dynamicHint.BottomBoundary = BottomBoundary.Value;

            if (LeftBoundary.HasValue)
                dynamicHint.LeftBoundary = LeftBoundary.Value;

            if (RightBoundary.HasValue)
                dynamicHint.RightBoundary = RightBoundary.Value;

            if (TargetX.HasValue)
                dynamicHint.TargetX = TargetX.Value;

            if (TargetY.HasValue)
                dynamicHint.TargetY = TargetY.Value;

            if (TopMargin.HasValue)
                dynamicHint.TopMargin = TopMargin.Value;

            if (BottomMargin.HasValue)
                dynamicHint.BottomMargin = BottomMargin.Value;

            if (LeftMargin.HasValue)
                dynamicHint.LeftMargin = LeftMargin.Value;

            if (RightMargin.HasValue)
                dynamicHint.RightMargin = RightMargin.Value;
        }

        public virtual DynamicHint GetHint()
        {
            DynamicHint dh = new DynamicHint();
            Apply(dh);
            return dh;
        }
    }
}
