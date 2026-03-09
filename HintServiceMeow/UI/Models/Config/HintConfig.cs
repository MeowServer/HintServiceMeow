namespace HintServiceMeow.UI.Models.Config
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Full configuration class for a <see cref="Hint"/> (fixed-position hint).
    /// Inherits the common base properties from <see cref="AbstractHintConfig"/> and adds
    /// all positional and alignment properties specific to <see cref="Hint"/>.
    /// <para>
    /// Designed for serialization (JSON/YAML). All properties are nullable so that only
    /// explicitly set values are applied when using <see cref="ApplyTo"/>.
    /// </para>
    /// </summary>
    public class HintConfig : AbstractHintConfig
    {
        /// <summary>
        /// Gets or sets the X (horizontal) coordinate of the hint.
        /// Valid range is roughly -1200 to 1200, including text length.
        /// Maps to <see cref="Hint.XCoordinate"/>.
        /// </summary>
        public float? XCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the Y (vertical) coordinate of the hint.
        /// Higher values place the hint lower on screen. Valid range is 0–1080.
        /// Maps to <see cref="Hint.YCoordinate"/>.
        /// </summary>
        public float? YCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the horizontal text alignment of the hint.
        /// Maps to <see cref="Hint.Alignment"/>.
        /// </summary>
        public HintAlignment? Alignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment reference point used when interpreting
        /// <see cref="YCoordinate"/>.
        /// Maps to <see cref="Hint.YCoordinateAlign"/>.
        /// </summary>
        public HintVerticalAlign? YCoordinateAlign { get; set; }

        /// <summary>
        /// Applies the non-null properties of this config to the given <paramref name="hint"/>.
        /// Only properties that have been explicitly set (non-null) are written; existing
        /// hint values are preserved for any property that remains null.
        /// </summary>
        /// <param name="hint">The target <see cref="Hint"/> to update.</param>
        public void ApplyTo(Hint hint)
        {
            // Apply common base properties (Id, SyncSpeed, FontSize, LineHeight, Text, Hide).
            ApplyBaseTo(hint);

            if (XCoordinate.HasValue)
                hint.XCoordinate = XCoordinate.Value;

            if (YCoordinate.HasValue)
                hint.YCoordinate = YCoordinate.Value;

            if (Alignment.HasValue)
                hint.Alignment = Alignment.Value;

            if (YCoordinateAlign.HasValue)
                hint.YCoordinateAlign = YCoordinateAlign.Value;
        }

        /// <summary>
        /// Creates a new <see cref="Hint"/> instance and populates it with all non-null
        /// properties from this config. Properties that are null fall back to the hint's
        /// built-in default values.
        /// </summary>
        /// <returns>A new <see cref="Hint"/> configured from this instance.</returns>
        public Hint ToHint()
        {
            var hint = new Hint();
            ApplyTo(hint);
            return hint;
        }
    }
}
