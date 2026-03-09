namespace HintServiceMeow.UI.Models.Config
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Full configuration class for a <see cref="DynamicHint"/> (auto-positioning hint).
    /// Inherits the common base properties from <see cref="AbstractHintConfig"/> and adds
    /// all layout, boundary, margin, priority, and strategy properties specific to
    /// <see cref="DynamicHint"/>.
    /// <para>
    /// Designed for serialization (JSON/YAML). All properties are nullable so that only
    /// explicitly set values are applied when using <see cref="ApplyTo"/>.
    /// </para>
    /// </summary>
    public class DynamicHintConfig : AbstractHintConfig
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

        // ── Behaviour ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the display priority of the hint. Higher priority hints are less
        /// likely to be displaced by other hints.
        /// Maps to <see cref="DynamicHint.Priority"/>.
        /// </summary>
        public HintPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets the fallback strategy applied when no valid display position is
        /// available within the hint's boundaries.
        /// Maps to <see cref="DynamicHint.Strategy"/>.
        /// </summary>
        public DynamicHintStrategy? Strategy { get; set; }

        // ── Methods ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the non-null properties of this config to the given <paramref name="hint"/>.
        /// Only properties that have been explicitly set (non-null) are written; existing
        /// hint values are preserved for any property that remains null.
        /// </summary>
        /// <param name="hint">The target <see cref="DynamicHint"/> to update.</param>
        public void ApplyTo(DynamicHint hint)
        {
            // Apply common base properties (Id, SyncSpeed, FontSize, LineHeight, Text, Hide).
            ApplyBaseTo(hint);

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

            if (Priority.HasValue)
                hint.Priority = Priority.Value;

            if (Strategy.HasValue)
                hint.Strategy = Strategy.Value;
        }

        /// <summary>
        /// Creates a new <see cref="DynamicHint"/> instance and populates it with all non-null
        /// properties from this config. Properties that are null fall back to the hint's
        /// built-in default values.
        /// </summary>
        /// <returns>A new <see cref="DynamicHint"/> configured from this instance.</returns>
        public DynamicHint ToDynamicHint()
        {
            var hint = new DynamicHint();
            ApplyTo(hint);
            return hint;
        }
    }
}
