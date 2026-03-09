namespace HintServiceMeow.UI.Models.Template
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// A visual/structural blueprint for a fixed-position <see cref="Hint"/> that carries no
    /// identity or visibility state. Contains all configurable display and positional
    /// properties except <c>Id</c> and <c>Hide</c>.
    /// <para>
    /// Use this template when you want to describe <em>how a hint looks and where it sits</em>
    /// without binding it to a specific identity or forcing it into a particular visibility
    /// state — for example, when applying a shared style to multiple hints.
    /// </para>
    /// <para>
    /// <see cref="HintTemplate"/> inherits from this class and extends it with <c>Id</c> and
    /// <c>Hide</c> for scenarios where full control is needed.
    /// </para>
    /// </summary>
    public class AnonymousHintTemplate : AbstractHintTemplate
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
        /// Applies all non-null properties from this template to <paramref name="hint"/>.
        /// Only explicitly set (non-null) values are written; the hint's existing values are
        /// preserved for any property that remains null.
        /// </summary>
        /// <param name="hint">The target <see cref="Hint"/> to update.</param>
        public virtual void ApplyTemplate(Hint hint)
        {
            // Apply common base properties: SyncSpeed, FontSize, LineHeight, Text.
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

        /// <summary>
        /// Instantiates a new <see cref="Hint"/> with default values, then applies all
        /// non-null properties from this template to it via <see cref="ApplyTemplate"/>.
        /// </summary>
        /// <returns>A new <see cref="Hint"/> configured from this template.</returns>
        public Hint GetHint()
        {
            var hint = new Hint();
            ApplyTemplate(hint);
            return hint;
        }
    }
}
