namespace HintServiceMeow.Core.Models.Parser.Style
{
    internal class TextMeshStyle
    {
        public TextMeshStyle(TextSegmentStyle charStyle, LineStyle lineStyle, float width, float height)
        {
            CharStyle = charStyle;
            LineStyle = lineStyle;
            Width = width;
            Height = height;
        }

        public static TextMeshStyle Default => new TextMeshStyle(TextSegmentStyle.Default, LineStyle.Default, 1440, 1080);

        public TextSegmentStyle CharStyle { get; set; }

        public LineStyle LineStyle { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }
    }
}
