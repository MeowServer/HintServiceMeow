namespace HintServiceMeow.Core.Models
{
    internal readonly struct CharacterInfo
    {
        public char Character { get; }
        public float FontSize { get; }
        public float Width { get; }
        public float Height { get; }
        public float VOffset { get; }

        public CharacterInfo(char character, float fontSize, float width, float height, float vOffset)
        {
            Character = character;
            FontSize = fontSize;
            Width = width;
            Height = height;
            VOffset = vOffset;
        }
    }
}
