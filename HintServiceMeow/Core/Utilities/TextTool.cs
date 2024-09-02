using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace HintServiceMeow.Core.Utilities
{
    internal static class TextTool
    {
        public static float GetTextWidth(string text, int size = 20)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            float width;
            var tuple = Tuple.Create(text, size);

            string noTagText = Regex.Replace(text, "<.*?>", string.Empty);
            string longestText = noTagText.Split('\n').OrderByDescending(x => x.Length).First();

            int halfSizeCharacters = longestText.Count(x => char.IsLetterOrDigit(x) || char.IsSymbol(x));
            int otherCharacters = longestText.Length - halfSizeCharacters;
            width = halfSizeCharacters * size / 1f + otherCharacters * size;

            return width;
        }

        public static float GetTextHeight(string text, int size = 20, float lineSpacing = 0)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            float height;
            var tuple = Tuple.Create(text, size);

            if (string.IsNullOrEmpty(text))
                return 0;

            height = text.Split('\n').Length * (size + lineSpacing);

            return height;
        }
    }

}
