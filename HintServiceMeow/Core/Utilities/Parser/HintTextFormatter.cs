namespace HintServiceMeow.Core.Utilities.Parser
{
    using System;
    using System.Globalization;
    using System.Text;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Utilities.Pools;

    /// <summary>
    /// Applies opt-in transformations to hint content before measuring and rendering it.
    /// </summary>
    internal static class HintTextFormatter
    {
        private const string LowercaseOpenTag = "<lowercase>";
        private const string LowercaseCloseTag = "</lowercase>";
        private const int ExplicitCaseTagCount = 4;

        private static readonly ICache<string, string> PreserveCaseCache = new Cache<string, string>(1000);

        private enum CaseControlTag
        {
            None = -1,
            Lowercase,
            Uppercase,
            Allcaps,
            Smallcaps,
            NoParse,
        }

        /// <summary>
        /// Gets the current text for a hint and applies its render-time formatting options.
        /// </summary>
        /// <param name="hint">The hint whose content should be formatted.</param>
        /// <returns>The text to measure and render.</returns>
        public static string GetText(AbstractHint hint)
        {
            string text = hint.Content.GetText() ?? string.Empty;
            return hint.PreserveCase ? PreserveCase(text) : text;
        }

        /// <summary>
        /// Preserves mixed casing under the game's default small-caps style by wrapping lowercase spans.
        /// Existing case and noparse tags remain authoritative.
        /// </summary>
        /// <param name="text">The rich text to transform.</param>
        /// <returns>The transformed rich text, or the original instance when no transformation is needed.</returns>
        public static string PreserveCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (PreserveCaseCache.TryGet(text, out string cachedText))
                return cachedText;

            int[]? explicitCaseDepths = null;
            int explicitCaseDepth = 0;
            bool noParse = false;
            StringBuilder? builder = null;
            int copyFrom = 0;
            int index = 0;

            while (index < text.Length)
            {
                if (text[index] == '<' && TryReadAngleToken(
                        text,
                        index,
                        out int tagEnd,
                        out CaseControlTag caseControlTag,
                        out bool isEndTag))
                {
                    if (caseControlTag == CaseControlTag.NoParse)
                    {
                        if (isEndTag)
                            noParse = false;
                        else if (!noParse)
                            noParse = true;
                    }
                    else if (!noParse && caseControlTag != CaseControlTag.None)
                    {
                        int caseTagIndex = (int)caseControlTag;
                        if (isEndTag)
                        {
                            if (explicitCaseDepths is not null && explicitCaseDepths[caseTagIndex] > 0)
                            {
                                explicitCaseDepths[caseTagIndex]--;
                                explicitCaseDepth--;
                            }
                        }
                        else
                        {
                            explicitCaseDepths ??= new int[ExplicitCaseTagCount];
                            explicitCaseDepths[caseTagIndex]++;
                            explicitCaseDepth++;
                        }
                    }

                    index = tagEnd + 1;
                    continue;
                }

                if (noParse || explicitCaseDepth > 0 || !char.IsLower(text[index]))
                {
                    index++;
                    continue;
                }

                builder ??= StringBuilderPool.Instance.Rent();
                builder.Append(text, copyFrom, index - copyFrom);
                builder.Append(LowercaseOpenTag);

                int lowercaseEnd = FindLowercaseSpanEnd(text, index);
                builder.Append(text, index, lowercaseEnd - index);
                builder.Append(LowercaseCloseTag);

                index = lowercaseEnd;
                copyFrom = lowercaseEnd;
            }

            if (builder is null)
                return text;

            builder.Append(text, copyFrom, text.Length - copyFrom);
            string result = StringBuilderPool.Instance.ToStringReturn(builder);
            PreserveCaseCache.Add(text, result);
            return result;
        }

        private static int FindLowercaseSpanEnd(string text, int startIndex)
        {
            int index = startIndex + 1;
            while (index < text.Length)
            {
                char character = text[index];
                if (character == '<' || char.IsUpper(character) ||
                    CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.TitlecaseLetter)
                {
                    break;
                }

                index++;
            }

            return index;
        }

        private static bool TryReadAngleToken(
            string text,
            int tagStart,
            out int tagEnd,
            out CaseControlTag caseControlTag,
            out bool isEndTag)
        {
            tagEnd = text.IndexOf('>', tagStart + 1);
            caseControlTag = CaseControlTag.None;
            isEndTag = false;

            if (tagEnd < 0)
                return false;

            if (SegmentEquals(text, tagStart, tagEnd, LowercaseOpenTag))
            {
                caseControlTag = CaseControlTag.Lowercase;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, LowercaseCloseTag))
            {
                caseControlTag = CaseControlTag.Lowercase;
                isEndTag = true;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "<uppercase>"))
            {
                caseControlTag = CaseControlTag.Uppercase;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "</uppercase>"))
            {
                caseControlTag = CaseControlTag.Uppercase;
                isEndTag = true;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "<allcaps>"))
            {
                caseControlTag = CaseControlTag.Allcaps;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "</allcaps>"))
            {
                caseControlTag = CaseControlTag.Allcaps;
                isEndTag = true;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "<smallcaps>"))
            {
                caseControlTag = CaseControlTag.Smallcaps;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "</smallcaps>"))
            {
                caseControlTag = CaseControlTag.Smallcaps;
                isEndTag = true;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "<noparse>"))
            {
                caseControlTag = CaseControlTag.NoParse;
            }
            else if (SegmentEquals(text, tagStart, tagEnd, "</noparse>"))
            {
                caseControlTag = CaseControlTag.NoParse;
                isEndTag = true;
            }

            return true;
        }

        private static bool SegmentEquals(string text, int start, int end, string value)
        {
            int length = end - start + 1;
            return length == value.Length &&
                   string.Compare(text, start, value, 0, length, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
