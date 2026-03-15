namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// YAML-serializable blueprint for a fixed-position <see cref="Hint"/>.
    /// Covers all configurable display and positional properties except <c>Id</c> and <c>Hide</c>.
    /// <para>
    /// <see cref="HintTemplate"/> extends this class with identity, visibility,
    /// and code-only properties (<c>AutoText</c>, transitions).
    /// </para>
    /// </summary>
    public class HintConfig : AbstractHintTemplate
    {
        /// <summary>
        /// Gets or sets the X coordinate. Valid range is roughly -1200 to 1200 including text length.
        /// Maps to <see cref="Hint.XCoordinate"/>.
        /// </summary>
        public float? XCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate. Higher values place the hint lower on screen. Valid range is 0–1080.
        /// Maps to <see cref="Hint.YCoordinate"/>.
        /// </summary>
        public float? YCoordinate { get; set; }

        /// <summary>Gets or sets the horizontal text alignment. Maps to <see cref="Hint.Alignment"/>.</summary>
        public HintAlignment? Alignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment reference point for <see cref="YCoordinate"/>.
        /// Maps to <see cref="Hint.YCoordinateAlign"/>.
        /// </summary>
        public HintVerticalAlign? YCoordinateAlign { get; set; }

        /// <summary>Applies all non-null properties to <paramref name="hint"/>.</summary>
        public virtual void Apply(Hint hint)
        {
            ApplyBaseTemplate(hint);

            if (XCoordinate.HasValue)
                hint.XCoordinate = XCoordinate.Value;

            if (YCoordinate.HasValue)
                hint.YCoordinate = YCoordinate.Value;

            if (Alignment.HasValue)
                hint.Alignment = Alignment.Value;

            if (YCoordinateAlign.HasValue)
                hint.YCoordinateAlign = YCoordinateAlign.Value;
        }

        /// <summary>Creates a new <see cref="Hint"/> and applies this template to it.</summary>
        public Hint GetHint()
        {
            var hint = new Hint();
            Apply(hint);
            return hint;
        }
    }
}
