using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Utilities.Parser
{
    internal static class TagChecker
    {
        private static readonly HashSet<string> ValidTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "align", "allcaps", "alpha", "b", "br", "color", "cspace", "font", "font-weight",
            "gradient", "i", "indent", "line-height", "line-indent", "lowercase", "margin", "mark",
            "mspace", "nobr", "noparse", "pos", "rotate", "s", "size", "smallcaps", "space", "sprite",
            "style", "sub", "sup", "u", "uppercase", "voffset", "width", "link",
        };

        private static readonly string[] KnownTags =
        [
            "a", "align", "allcaps", "alpha", "b", "br", "color", "cspace", "font", "font-weight",
            "gradient", "i", "indent", "line-height", "line-indent", "lowercase", "margin", "mark",
            "mspace", "nobr", "noparse", "pos", "rotate", "s", "size", "smallcaps", "space", "sprite",
            "style", "sub", "sup", "u", "uppercase", "voffset", "width", "link"
        ];

        private static readonly HashSet<string> SelfClosingTags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "br",
            "sprite",
            "space",
            "pos",
            "alpha",
        };

        public static bool IsValidTag(string tagName)
        {
            return ValidTags.Contains(tagName);
        }

        public static bool IsSelfClosingTag(string tagName)
        {
            return SelfClosingTags.Contains(tagName);
        }

        public static string TryMatchValidTag(string rawText, int start, int length)
        {
            for (int i = 0; i < KnownTags.Length; i++)
            {
                string candidate = KnownTags[i];
                if (candidate.Length != length)
                    continue;

                bool match = true;
                for (int j = 0; j < length; j++)
                {
                    if (char.ToLowerInvariant(rawText[start + j]) != candidate[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return candidate;
            }

            return null; // Illegal Tag
        }
    }
}
