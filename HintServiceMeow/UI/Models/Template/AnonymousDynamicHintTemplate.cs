namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// A visual/structural blueprint for a <see cref="DynamicHint"/> that carries no
    /// identity or visibility state. Contains all configurable layout and behavioural
    /// properties — boundaries, target coordinates, margins, priority and strategy —
    /// plus the inherited base properties (font, text, sync speed) from
    /// <see cref="AbstractHintTemplate"/>, but intentionally excludes <c>Id</c> and <c>Hide</c>.
    /// <para>
    /// Use this template when you want to describe <em>how and where a dynamic hint
    /// positions itself</em> without binding it to a specific identity or forcing a
    /// particular visibility state.
    /// </para>
    /// <para>
    /// <see cref="DynamicHintTemplate"/> inherits from this class and extends it with
    /// <c>Id</c> and <c>Hide</c> for scenarios where full control is needed.
    /// </para>
    /// </summary>
    public class AnonymousDynamicHintTemplate : AbstractHintTemplate
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
        /// Gets or sets the display priority of the hint. Higher-priority hints are less
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
        /// Applies all non-null properties from this template to <paramref name="hint"/>.
        /// Only explicitly set (non-null) values are written; the hint's existing values are
        /// preserved for any property that remains null.
        /// </summary>
        /// <param name="hint">The target <see cref="DynamicHint"/> to update.</param>
        public virtual void ApplyTemplate(DynamicHint hint)
        {
            // Apply common base properties: SyncSpeed, FontSize, LineHeight, Text.
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
        /// Instantiates a new <see cref="DynamicHint"/> with default values, then applies
        /// all non-null properties from this template to it via <see cref="ApplyTemplate"/>.
        /// </summary>
        /// <returns>A new <see cref="DynamicHint"/> configured from this template.</returns>
        public DynamicHint GetDynamicHint()
        {
            var hint = new DynamicHint();
            ApplyTemplate(hint);
            return hint;
        }
    }
}
