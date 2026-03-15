using HintServiceMeow.Core.Models.Parser.Style;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Models.Parser
{
    internal struct TextSegment
    {
        private float totalWidthWithFontSize;
        private string text;

        public TextSegment(string segment, TextSegmentStyle style)
        {
            this.text = segment;
            totalWidthWithFontSize = 0f;
            for (int i = 0; i < segment.Length; i++)
            {
                totalWidthWithFontSize += FontTool.Instance.GetCharWidth(segment[i], style.FontSize);
            }

            Style = style;
        }

        public string Text => text;

        public TextSegmentStyle Style { get; set; }

        public float Width
        {
            get
            {
                if (CustomWidth.HasValue)
                    return CustomWidth.Value;

                return Style.GetWidth(totalWidthWithFontSize, text.Length);
            }
        }

        public float Height => Style.GetHeight();

        public float? CustomWidth { get; set; } = null;
    }
}
