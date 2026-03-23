namespace HintServiceMeow.Core.Utilities.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Parser;
    using HintServiceMeow.Core.Utilities.Pools;

    internal class Tokenizer
    {
        private readonly object tokenizerLock = new object();
        private List<Token> tokenList = new();
        private int index = 0;
        private string? rawText = null;
        private StringBuilder? sb = null;

        public List<Token> Tokenize(string raw, Tuple<string, IParameter>[] registeredParameters)
        {
            lock (tokenizerLock)
            {
                try
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

                        if (TryHandleParameter(registeredParameters))
                            continue;

                        if (raw[index] == '\n')
                        {
                            PackTextInSbAndAdd(Token.GetLineBreak());
                            index++;
                            continue;
                        }

                        sb.Append(raw[index]);
                        index++;
                    }

                    PackTextInSb();

                    List<Token> result = tokenList;
                    tokenList = new List<Token>(result.Count);
                    return result;
                }
                finally
                {
                    StringBuilderPool.Instance.Return(sb);
                    sb = null;
                }
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
                    PackTextInSbAndAdd(Token.GetLineBreak());
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
        /// Attempts to process a rich tag in the current context. If failed, return false. If success, return true and move the index to the next character after the rich tag.
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

            // Cut out the tag
            int tagStart = index + 1;
            int tagEnd = rawText.IndexOf('>', tagStart) - 1;
            // No closing >
            if (tagEnd < 0)
            {
                return false;
            }

            // Cut out the tag ( without < and > )
            // Check if is close tag or open tag
            bool isCloseTag = rawText[tagStart] == '/';
            string? tagName, tagParameter = null;

            // Is close tag, remove the closing mark
            if (isCloseTag)
            {
                // Skip first char(/) and cut out the tag name
                tagName = TagChecker.TryMatchValidTag(rawText, tagStart + 1, tagEnd - tagStart);
            }
            else
            {
                // Is open tag, get the parameter if there is parameter
                // Find 1 equal sign after the start
                int equalSignIndex = rawText.IndexOf('=', tagStart);

                // Not equal sign within the tag
                if (equalSignIndex == -1 || equalSignIndex > tagEnd)
                {
                    tagName = TagChecker.TryMatchValidTag(rawText, tagStart, tagEnd - tagStart + 1);
                }
                else
                {
                    tagName = TagChecker.TryMatchValidTag(rawText, tagStart, equalSignIndex - tagStart);
                    tagParameter = rawText.Substring(equalSignIndex + 1, tagEnd - equalSignIndex);
                }
            }

            // Check if the tag is valid
            if (tagName == null)
            {
                return false;
            }

            // Handle the tag. Handle <br> first as it is special
            if (tagName.Equals("br", StringComparison.OrdinalIgnoreCase))
            {
                PackTextInSbAndAdd(Token.GetLineBreak());
            }
            else
            {
                if (isCloseTag)
                {
                    PackTextInSbAndAdd(Token.GetTag(RichTextTokenType.CloseTag, tagName, tagParameter));
                }
                else if (TagChecker.IsSelfClosingTag(tagName))
                {
                    PackTextInSbAndAdd(Token.GetTag(RichTextTokenType.SelfCloseTag, tagName, tagParameter));
                }
                else
                {
                    PackTextInSbAndAdd(Token.GetTag(RichTextTokenType.OpenTag, tagName, tagParameter));
                }
            }

            index = tagEnd + 2; // Move to character after the > of the tag
            return true;
        }

        /// <summary>
        /// Attempts to handle a parameter in the input text by matching it against the specified registered parameters.
        /// If failed, return false. If success, return true and move the index to the next character after the parameter.
        /// </summary>
        /// <param name="registeredParameters">An array of tuples containing parameter names and their corresponding hint parameter objects to match
        /// against the input text.</param>
        /// <returns>true if a matching parameter is found and handled; otherwise, false.</returns>
        private bool TryHandleParameter(Tuple<string, IParameter>[] registeredParameters)
        {
            if (rawText![index] != '{')
            {
                return false;
            }

            int start = index + 1;

            // Find the end of the parameter
            int endParamIndex = rawText.IndexOf('}', start);
            if (endParamIndex == -1)
            {
                return false;
            }

            // Cut out the parameter ( without { and } )
            string paramContent = rawText.Substring(start, endParamIndex - start);

            // Handle parameter if it is a tag
            for (int i = 0; i < registeredParameters.Length; i++)
            {
                if (string.Equals(paramContent, registeredParameters[i].Item1, StringComparison.OrdinalIgnoreCase))
                {
                    PackTextInSbAndAdd(Token.GetParameter(registeredParameters[i].Item2));
                    index = endParamIndex + 1; // Move to character after the parameter
                    return true;
                }
            }

            // TODO: Check if need to add StringHintParameter to replace original text when no matching parameter found.
            return false; // No matching parameter found, treat it as normal text
        }

        private void PackTextInSbAndAdd(Token token)
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

            tokenList.Add(Token.GetText(text));
            sb.Clear();
        }
    }
}
