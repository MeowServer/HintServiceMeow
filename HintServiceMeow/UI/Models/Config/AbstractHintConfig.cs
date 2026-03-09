namespace HintServiceMeow.UI.Models.Config
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Base configuration class containing common configurable properties shared by all hint types.
    /// Use this as a data-transfer object for creating or updating hints via serialization (JSON/YAML).
    /// <para>
    /// Only user-facing, configurable properties are included. Internal runtime state such as
    /// <c>Guid</c>, <c>UpdateAnalyser</c>, and transition states are intentionally omitted.
    /// </para>
    /// <para>
    /// All properties are nullable. When calling <c>ApplyTo</c>, only non-null properties are
    /// written to the target hint, leaving the hint's existing values for any property that is null.
    /// </para>
    /// </summary>
    public abstract class AbstractHintConfig
    {
        /// <summary>
        /// Gets or sets the logical identifier used to group or retrieve the hint.
        /// Maps to <see cref="AbstractHint.Id"/>.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the synchronization speed that controls how quickly the hint's updates
        /// are sent to the display.
        /// Maps to <see cref="AbstractHint.SyncSpeed"/>.
        /// </summary>
        public HintSyncSpeed? SyncSpeed { get; set; }

        /// <summary>
        /// Gets or sets the font size of the hint text.
        /// Maps to <see cref="AbstractHint.FontSize"/>.
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        /// Gets or sets the line height multiplier for the hint text.
        /// Maps to <see cref="AbstractHint.LineHeight"/>.
        /// </summary>
        public float? LineHeight { get; set; }

        /// <summary>
        /// Gets or sets the plain-text content of the hint.
        /// Setting this will replace the hint's content with a <c>StringContent</c> instance.
        /// Maps to <see cref="AbstractHint.Text"/>.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets whether the hint is hidden from the player's display.
        /// Maps to <see cref="AbstractHint.Hide"/>.
        /// </summary>
        public bool? Hide { get; set; }

        /// <summary>
        /// Applies the non-null properties of this config to the given <paramref name="hint"/>,
        /// leaving existing hint values unchanged for any property that is null in the config.
        /// </summary>
        /// <param name="hint">The target hint to update.</param>
        protected void ApplyBaseTo(AbstractHint hint)
        {
            if (Id != null)
                hint.Id = Id;

            if (SyncSpeed.HasValue)
                hint.SyncSpeed = SyncSpeed.Value;

            if (FontSize.HasValue)
                hint.FontSize = FontSize.Value;

            if (LineHeight.HasValue)
                hint.LineHeight = LineHeight.Value;

            if (Text != null)
                hint.Text = Text;

            if (Hide.HasValue)
                hint.Hide = Hide.Value;
        }
    }
}
