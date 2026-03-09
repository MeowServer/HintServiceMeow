namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Base class for all hint templates. Contains the common visual/structural properties
    /// shared by every hint type: text content, font, line height, and sync speed.
    /// <para>
    /// Runtime-only state (<c>Guid</c>, <c>UpdateAnalyser</c>, transition states,
    /// <c>PreviousXCoordinate</c>, etc.) is intentionally excluded from all templates.
    /// </para>
    /// <para>
    /// All properties are nullable. <see cref="ApplyBaseTemplate"/> writes only the
    /// properties that have been explicitly set (non-null), leaving the hint's existing
    /// values untouched for anything that remains null.
    /// </para>
    /// </summary>
    public abstract class AbstractHintTemplate
    {
        /// <summary>
        /// Gets or sets the synchronization speed controlling how quickly the hint's
        /// updates are dispatched to the display.
        /// Maps to <see cref="AbstractHint.SyncSpeed"/>.
        /// </summary>
        public HintSyncSpeed? SyncSpeed { get; set; }

        /// <summary>
        /// Gets or sets the font size of the hint text.
        /// Maps to <see cref="AbstractHint.FontSize"/>.
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        /// Gets or sets the line-height offset for the hint text.
        /// Maps to <see cref="AbstractHint.LineHeight"/>.
        /// </summary>
        public float? LineHeight { get; set; }

        /// <summary>
        /// Gets or sets the plain-text content of the hint.
        /// Setting this replaces the hint's content with a <c>StringContent</c> instance.
        /// Maps to <see cref="AbstractHint.Text"/>.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Applies all non-null base properties (<see cref="SyncSpeed"/>, <see cref="FontSize"/>,
        /// <see cref="LineHeight"/>, <see cref="Text"/>) from this template to <paramref name="hint"/>.
        /// Intended to be called by concrete subclass <c>ApplyTemplate</c> implementations.
        /// </summary>
        /// <param name="hint">The target hint to update.</param>
        protected void ApplyBaseTemplate(AbstractHint hint)
        {
            if (SyncSpeed.HasValue)
                hint.SyncSpeed = SyncSpeed.Value;

            if (FontSize.HasValue)
                hint.FontSize = FontSize.Value;

            if (LineHeight.HasValue)
                hint.LineHeight = LineHeight.Value;

            if (Text != null)
                hint.Text = Text;
        }
    }
}
