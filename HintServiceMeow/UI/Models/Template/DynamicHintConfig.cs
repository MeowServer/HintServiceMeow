namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// YAML-serializable blueprint for a <see cref="DynamicHint"/>.
    /// Covers boundaries, target coordinates, margins, priority, and strategy,
    /// plus the inherited base properties from <see cref="AbstractHintTemplate"/>.
    /// Excludes <c>Id</c>, <c>AutoText</c>, <c>transitions</c> and <c>Hide</c>.
    /// <para>
    /// <see cref="DynamicHintTemplate"/> extends this class with identity, visibility,
    /// and code-only properties (<c>AutoText</c>, transitions).
    /// </para>
    /// </summary>
    public class DynamicHintConfig : AbstractHintTemplate
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

        // ── Behaviour ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the display priority. Higher-priority hints are less likely to be displaced.
        /// Maps to <see cref="DynamicHint.Priority"/>.
        /// </summary>
        public HintPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets the fallback strategy when no valid position is available.
        /// Maps to <see cref="DynamicHint.Strategy"/>.
        /// </summary>
        public DynamicHintStrategy? Strategy { get; set; }

        // ── Methods ───────────────────────────────────────────────────────────────

        /// <summary>Applies all non-null properties to <paramref name="hint"/>.</summary>
        public virtual void Apply(DynamicHint hint)
        {
            ApplyBaseTemplate(hint);

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
        /// Creates a new <see cref="DynamicHint"/> and applies this template to it.
        /// </summary>
        /// <returns>A dynamic hint with template's value.</returns>
        public DynamicHint GetDynamicHint()
        {
            var hint = new DynamicHint();
            Apply(hint);
            return hint;
        }
    }
}
