using System.Collections.Generic;
using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Parser
{
    internal struct LineInfo
    {
        public List<CharInfo> CharacterInfos { get; }

        public LineStyle Style { get; }

        public string CleanText { get; }

        public float Width
        {
            get
            {
                float totalWidth = 0;
                for (int i = 0; i < CharacterInfos.Count; i++)
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
                for (int i = 0; i < CharacterInfos.Count; i++)
                {
                    if (CharacterInfos[i].Height > highestHeight)
                        highestHeight = CharacterInfos[i].Height;
                }

                return highestHeight;
            }
        }

        public LineInfo(List<CharInfo> characterInfos, LineStyle style, string cleanText)
        {
            CharacterInfos = characterInfos;
            Style = style;
            CleanText = cleanText;
        }
    }
}
