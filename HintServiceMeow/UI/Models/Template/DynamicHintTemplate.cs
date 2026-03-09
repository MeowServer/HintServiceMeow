namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Full template for a <see cref="DynamicHint"/>. Extends
    /// <see cref="AnonymousDynamicHintTemplate"/> with identity and visibility control
    /// by adding <c>Id</c> and <c>Hide</c>.
    /// <para>
    /// Use this template when you need complete control over a dynamic hint, including
    /// its logical identifier and whether it is visible.
    /// </para>
    /// <para>
    /// Designed for serialization (JSON/YAML). All properties are nullable so only
    /// explicitly set values are applied by <see cref="ApplyTemplate"/>.
    /// </para>
    /// </summary>
    public class DynamicHintTemplate : AnonymousDynamicHintTemplate
    {
        /// <summary>
        /// Gets or sets the logical identifier used to group or retrieve the hint.
        /// Maps to <see cref="AbstractHint.Id"/>.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets whether the hint is hidden from the player's display.
        /// Maps to <see cref="AbstractHint.Hide"/>.
        /// </summary>
        public bool? Hide { get; set; }

        /// <summary>
        /// Applies all non-null properties from this template to <paramref name="hint"/>,
        /// including the base visual/layout properties (via
        /// <see cref="AnonymousDynamicHintTemplate.ApplyTemplate"/>) as well as
        /// <see cref="Id"/> and <see cref="Hide"/>.
        /// Only explicitly set (non-null) values are written; the hint's existing values are
        /// preserved for any property that remains null.
        /// </summary>
        /// <param name="hint">The target <see cref="DynamicHint"/> to update.</param>
        public override void ApplyTemplate(DynamicHint hint)
        {
            // Apply anonymous base properties:
            // SyncSpeed, FontSize, LineHeight, Text, boundaries, targets, margins, Priority, Strategy.
            base.ApplyTemplate(hint);

            if (Id != null)
                hint.Id = Id;

            if (Hide.HasValue)
                hint.Hide = Hide.Value;
        }

        // GetDynamicHint() is inherited from AnonymousDynamicHintTemplate.
        // It calls the virtual ApplyTemplate, so this override is picked up automatically.
    }
}
