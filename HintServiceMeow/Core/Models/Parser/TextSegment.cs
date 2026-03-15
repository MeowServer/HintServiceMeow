using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Parser
{
    internal struct TextSegment
    {
        private string text;
        private float width;

        public TextSegment(string segment, float width, TextSegmentStyle style)
        {
            this.text = segment;

            Style = style;
        }

        public string Text => text;

        public TextSegmentStyle Style { get; set; }

        public float Width
        {
            get
            {
                //if (CustomWidth.HasValue)
                //    return CustomWidth.Value;

                return width;
            }
        }

        public float Height => Style.GetHeight();

        // public float? CustomWidth { get; set; } = null;
    }
}
