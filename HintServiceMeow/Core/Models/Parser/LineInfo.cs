namespace HintServiceMeow.Core.Models.Parser
{
    using HintServiceMeow.Core.Models.Parser.Style;

    internal struct LineInfo
    {
        private readonly float emptyLineHeight;

        public LineInfo(TextSegment[] characterInfos, LineStyle style, string cleanText, float emptyLineHeight = 0f)
        {
            CharacterInfos = characterInfos;
            Style = style;
            CleanText = cleanText;
            this.emptyLineHeight = emptyLineHeight;
        }

        public TextSegment[] CharacterInfos { get; }

        public LineStyle Style { get; }

        public string CleanText { get; }

        public float Width
        {
            get
            {
                float totalWidth = 0;
                for (int i = 0; i < CharacterInfos.Length; i++)
                {
                    totalWidth += CharacterInfos[i].Width;
                }

                totalWidth += Style.Indent + Style.MarginLeft + Style.MarginRight;

                return totalWidth;
            }
        }

        public float Height
        {
            get
            {
                if (Style.LineHeight != null)
                    return Style.LineHeight.Value;

                float highestHeight = 0f;
                for (int i = 0; i < CharacterInfos.Length; i++)
                {
                    if (CharacterInfos[i].Height > highestHeight)
                        highestHeight = CharacterInfos[i].Height;
                }

                // An empty line (e.g. a blank line between paragraphs, or a trailing line break)
                // has no characters, so its measured height is 0. Returning 0 would collapse the
                // blank line and make surrounding lines run together. Fall back to the default
                // line height so blank lines keep their vertical space.
                if (highestHeight <= 0f)
                    return emptyLineHeight;

                return highestHeight;
            }
        }
    }
}
