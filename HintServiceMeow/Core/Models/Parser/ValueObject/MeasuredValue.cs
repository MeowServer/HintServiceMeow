namespace HintServiceMeow.Core.Models.Parser.ValueObject
{
    using HintServiceMeow.Core.Enum;

    /// <summary>
    /// Represents a measurement value with an associated unit (pixel, font unit, or percentage).
    /// </summary>
    internal struct MeasuredValue
    {
        /// <summary>The numeric value of the measurement.</summary>
        public float Value;

        /// <summary>The unit in which <see cref="Value"/> is expressed.</summary>
        public MeasureUnit Unit;

        /// <summary>
        /// Attempts to convert this measurement to an absolute pixel value.
        /// </summary>
        /// <param name="fontSize">The current font size in pixels, used when the unit is <see cref="MeasureUnit.FontUnit"/>.</param>
        /// <param name="referenceSize">The reference size in pixels, used when the unit is <see cref="MeasureUnit.Percentage"/>.</param>
        /// <param name="pixels">When this method returns, contains the pixel value if conversion succeeded; otherwise null.</param>
        /// <returns>true if <paramref name="pixels"/> has a value; otherwise false.</returns>
        public bool TryGetPixels(float? fontSize, float? referenceSize, out float? pixels)
        {
            pixels = Unit switch
            {
                MeasureUnit.Pixel => Value,
                MeasureUnit.FontUnit => Value * fontSize,
                MeasureUnit.Percentage => Value * referenceSize / 100f,
                _ => Value,
            };

            return pixels != null;
        }
    }
}
