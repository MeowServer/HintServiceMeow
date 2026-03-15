using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Parser
{
    internal struct LineInfo
    {
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

                return highestHeight;
            }
        }

        public LineInfo(TextSegment[] characterInfos, LineStyle style, string cleanText)
        {
            CharacterInfos = characterInfos;
            Style = style;
            CleanText = cleanText;
        }
    }
}
