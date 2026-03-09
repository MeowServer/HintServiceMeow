namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// A lightweight, position-only template for a fixed-position <see cref="Hint"/>.
    /// Contains only the four positional and alignment properties; carries no content,
    /// font, identity, or visibility state.
    /// <para>
    /// Use this template when you only need to reposition an existing hint without
    /// altering any other properties.
    /// Intentionally does <b>not</b> inherit from <see cref="AbstractHintTemplate"/>
    /// to keep the surface area minimal.
    /// </para>
    /// <para>
    /// All properties are nullable. Only non-null values are applied by
    /// <see cref="ApplyTemplate"/>.
    /// </para>
    /// </summary>
    public class HintPositionTemplate
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
        /// Applies the non-null positional properties of this template to the given
        /// <paramref name="hint"/>. Existing hint values are preserved for any property
        /// that remains null.
        /// </summary>
        /// <param name="hint">The target <see cref="Hint"/> to reposition.</param>
        public void ApplyTemplate(Hint hint)
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
