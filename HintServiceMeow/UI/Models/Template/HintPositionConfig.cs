namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Position-only template for a fixed-position <see cref="Hint"/>.
    /// Use when you only need to reposition an existing hint without touching any other properties.
    /// Intentionally does not inherit from <see cref="AbstractHintTemplate"/> to keep the surface area minimal.
    /// </summary>
    public class HintPositionConfig
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

        /// <summary>Applies all non-null positional properties to <paramref name="hint"/>.</summary>
        public virtual void Apply(Hint hint)
        {
            if (XCoordinate.HasValue)
                hint.XCoordinate = XCoordinate.Value;

            if (YCoordinate.HasValue)
                hint.YCoordinate = YCoordinate.Value;

            if (Alignment.HasValue)
                hint.Alignment = Alignment.Value;

            if (YCoordinateAlign.HasValue)
                hint.YCoordinateAlign = YCoordinateAlign.Value;
        }
    }
}
