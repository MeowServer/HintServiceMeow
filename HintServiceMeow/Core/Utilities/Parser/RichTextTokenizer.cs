namespace HintServiceMeow.Core.Utilities.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Parser;
    using HintServiceMeow.Core.Utilities.Pools;

    internal class RichTextTokenizer
    {
        private readonly object tokenizerLock = new object();
        private readonly List<RichTextToken> tokenList = new();
        private int index = 0;
        private string? rawText = null;
        private StringBuilder? sb = null;

        public RichTextToken[] Tokenize(string raw)
        {
            lock (tokenizerLock)
            {
                sb = StringBuilderPool.Instance.Rent();
                tokenList.Clear();
                index = 0;
                rawText = raw;

                while (index < rawText.Length)
                {
                    // Handle Escape Character. If not a excape character, skip. If is a excape character, start over at the end of the character.
                    if (TryHandleEscapeCharacter())
                        continue;

                    // Handle Rich Tag. If not a rich tag, skip. If is a rich tag, start over at the end of tag.
                    if (TryHandleRichTag())
                        continue;

                    sb.Append(raw[index]);
                    index++;
                }

                PackTextInSb();

                StringBuilderPool.Instance.Return(sb);
                sb = null;

                RichTextToken[] result = tokenList.ToArray();
                tokenList.Clear();
                return result;
            }
        }

        /// <summary>
        /// Handle Escape Character. If failed, return false. If success, return true and move the index to the next character after the escape character.
        /// </summary>
        private bool TryHandleEscapeCharacter()
        {
            if (rawText![index] != '\\')
            {
                return false;
            }

            if (rawText.Length <= index + 1)
            {
                return false;
            }

            switch (rawText[index + 1])
            {
                case 'n':
                    PackTextInSbAndAdd(RichTextToken.GetLineBreak());
                    break;
                case '\\':
                    sb!.Append('\\');
                    break;
                default:
                    return false; // Failed
            }

            index += 2; // Move to character after the escape character

            return true;
        }

        /// <summary>
        /// Attempts to process a rich tag in the current context. If failed, return false, If success, return true and move the index to the next character after the rich tag.
        /// </summary>
        /// <returns>true if a rich tag was successfully handled; otherwise, false.</returns>
        private bool TryHandleRichTag()
        {
            if (rawText![index] != '<')
            {
                return false;
            }

            if (rawText.Length <= index + 1)
            {
                return false;
            }

            int start = index + 1;

            // Find the end of the tag
            int endTagIndex = rawText.IndexOf('>', start);
            if (endTagIndex == -1)
            {
                return false;
            }

            // Cut out the tag ( without < and > )
            string tagContent = rawText.Substring(start, endTagIndex - start);

            // Check if is close tag or open tag
            bool isCloseTag = tagContent.StartsWith("/");
            string? tagName, tagParameter = null;
            if (isCloseTag) // Is close tag, remove the closing mark
            {
                tagName = tagContent.Substring(1).Trim();
            }
            else // Is open tag, get the parameter if there is parameter
            {
                // Check if has value
                int equalSignIndex = tagContent.IndexOf('=');

                if (equalSignIndex == -1)
                {
                    tagName = tagContent;
                }
                else
                {
                    tagName = tagContent.Substring(0, equalSignIndex);
                    tagParameter = tagContent.Substring(equalSignIndex + 1);
                }
            }

            // Check if the tag is valid
            if (!TagChecker.IsValidTag(tagName))
            {
                return false;
            }

            // Handle the tag. Handle <br> first as it is special
            if (tagName.Equals("br", StringComparison.OrdinalIgnoreCase))
            {
                PackTextInSbAndAdd(RichTextToken.GetLineBreak());
            }
            else
            {
                if (TagChecker.IsSelfClosingTag(tagName))
                {
                    PackTextInSbAndAdd(RichTextToken.GetTag(RichTextTokenType.SelfCloseTag, tagName, tagParameter));
                }
                else if (isCloseTag)
                {
                    PackTextInSbAndAdd(RichTextToken.GetTag(RichTextTokenType.CloseTag, tagName, tagParameter));
                }
                else
                {
                    PackTextInSbAndAdd(RichTextToken.GetTag(RichTextTokenType.OpenTag, tagName, tagParameter));
                }
            }

            index = endTagIndex + 1; // Move to character after the tag
            return true;
        }

        private void PackTextInSbAndAdd(RichTextToken token)
        {
            PackTextInSb();
            tokenList.Add(token);
        }

        private void PackTextInSb()
        {
            string text = sb!.ToString();

            if (text == string.Empty)
            {
                return;
            }

            tokenList.Add(RichTextToken.GetText(text));
            sb.Clear();
        }
    }
}
