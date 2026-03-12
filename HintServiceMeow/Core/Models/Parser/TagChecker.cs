using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Models.Parser
{
    internal static class TagChecker
    {
        private static readonly object TagListLock = new object();
        private static readonly HashSet<string> ValidTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "align", "allcaps", "alpha", "b", "br", "color", "cspace", "font", "font-weight",
            "gradient", "i", "indent", "line-height", "line-indent", "lowercase", "margin", "mark",
            "mspace", "nobr", "noparse", "pos", "rotate", "s", "size", "smallcaps", "space", "sprite",
            "style", "sub", "sup", "u", "uppercase", "voffset", "width", "link"
        };

        private static readonly HashSet<string> SelfClosingTags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "br",
            "sprite",
            "space",
            "pos",
            "alpha"
        };

        public static bool IsValidTag(string tagName)
        {
            lock (TagListLock)
            {
                return ValidTags.Contains(tagName);
            }
        }

        public static bool IsSelfClosingTag(string tagName)
        {
            lock (TagListLock)
            {
                return SelfClosingTags.Contains(tagName);
            }
        }
    }
}
