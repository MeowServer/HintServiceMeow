namespace HintServiceMeow.Core.Models.Parser
{
    using HintServiceMeow.Core.Models.Parser.Style;

    internal struct TextSegment
    {
        private string text;
        private float width;

        public TextSegment(string segment, float width, TextSegmentStyle style)
        {
            this.text = segment;
            this.width = width;
            Style = style;
        }

        public string Text => text;

        public TextSegmentStyle Style { get; set; }

        public float Width => width;

        public float Height => Style.GetHeight();
    }
}
