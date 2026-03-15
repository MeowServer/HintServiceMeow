using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Models.Parser.ValueObject
{
    internal struct MeasuredValue
    {
        public float Value;
        public MeasureUnit Unit;

        public bool TryGetPixels(float? fontSize, float? referenceSize, out float? pixels)
        {
            pixels = Unit switch
            {
                MeasureUnit.Pixel => Value,
                MeasureUnit.FontUnit => Value * fontSize,
                MeasureUnit.Percentage => Value * referenceSize / 100f,
                _ => Value
            };

            return pixels != null;
        }
    }
}
