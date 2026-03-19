namespace HintServiceMeow.Core.Models.Parser.ValueObject
{
    internal struct Color
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;

        public Color(byte red, byte green, byte blue, byte alpha = 255)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }
    }
}
