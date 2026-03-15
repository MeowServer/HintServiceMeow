namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Base class for all hint templates. Covers the properties shared by every hint type.
    /// <para>
    /// All properties are nullable; <see cref="ApplyBaseTemplate"/> only writes non-null values,
    /// leaving the hint's existing values untouched for anything that remains null.
    /// </para>
    /// <para>
    /// Runtime-only state (<c>Guid</c>, <c>UpdateAnalyser</c>, 
    /// transition states) is excluded. Behaviour-only properties
    /// (<c>AutoText</c>, <c>Transition</c>) are available on the full template subclasses
    /// and are marked <c>[YamlIgnore]</c>.
    /// </para>
    /// </summary>
    public abstract class AbstractHintTemplate
    {
        /// <summary>Gets or sets the sync speed. Maps to <see cref="AbstractHint.SyncSpeed"/>.</summary>
        public HintSyncSpeed? SyncSpeed { get; set; }

        /// <summary>Gets or sets the font size. Maps to <see cref="AbstractHint.FontSize"/>.</summary>
        public int? FontSize { get; set; }

        /// <summary>Gets or sets the line-height offset. Maps to <see cref="AbstractHint.LineHeight"/>.</summary>
        public float? LineHeight { get; set; }

        /// <summary>
        /// Gets or sets the plain-text content. Use a static text as hint's content.
        /// Maps to <see cref="AbstractHint.Text"/>.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Applies all non-null base properties to <paramref name="hint"/>.
        /// Intended to be called by concrete subclass <c>ApplyTemplate</c> implementations.
        /// </summary>
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
