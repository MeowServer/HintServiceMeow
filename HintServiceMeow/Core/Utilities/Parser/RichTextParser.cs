using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Use to calculate the rich text's width and height.
    /// </summary>
    internal class RichTextParser
    {
        private Stack<float> _fontSizeStack = new Stack<float>();
        private bool _bold = false;
        private bool _italic = false;

        public float GetTextWidth(string text, int size = 20)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            string noTagText = Regex.Replace(text, "<.*?>", string.Empty);
            string longestText = noTagText.Split('\n').OrderByDescending(x => x.Length).First();

            int halfSizeCharacters = longestText.Count(x => char.IsLetterOrDigit(x) || char.IsSymbol(x));
            int otherCharacters = longestText.Length - halfSizeCharacters;
            float width = halfSizeCharacters * size / 1f + otherCharacters * size;

            return width;
        }

        public float GetTextHeight(string text, int size = 20, float lineSpacing = 0)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            float height = text.Split('\n').Length * (size + lineSpacing);

            return height;
        }
    }
}
