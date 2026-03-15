using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.Parser
{
    internal struct Token
    {
        public RichTextTokenType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag. Null if the token is not a tag, or the tag is a line break.
        /// </summary>
        public string? TagName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value associated with the tag. Null if the token is not a tag, or the tag is a line break.
        /// </summary>
        public string? TagValue { get; set; } = null;

        /// <summary>
        /// Gets or sets the text content of the token. Only valid when the token type is Text. Null if the token is not a text token.
        /// </summary>
        public string? Text { get; set; } = null;

        /// <summary>
        /// Gets or sets the hint parameter. Only valid when token type is Parameter. Null if the token is not a parameter token.
        /// </summary>
        public IHintParameter? Parameter { get; set; } = null;

        public Token(RichTextTokenType type, string? tagName = null, string? tagValue = null, string? text = null, IHintParameter? parameter = null)
        {
            Type = type;
            TagName = tagName;
            TagValue = tagValue;
            Text = text;
            Parameter = parameter;
        }

        public static Token GetText(string text)
        {
            return new Token(RichTextTokenType.Text, text: text);
        }

        public static Token GetTag(RichTextTokenType type, string tagName, string? tagValue)
        {
            return new Token(type, tagName: tagName.ToLowerInvariant(), tagValue: tagValue?.ToLowerInvariant());
        }

        public static Token GetLineBreak()
        {
            return new Token(RichTextTokenType.LineBreak);
        }

        public static Token GetParameter(IHintParameter parameter)
        {
            return new Token(RichTextTokenType.Parameter, parameter: parameter);
        }

        public override string ToString()
        {
            return Type switch
            {
                RichTextTokenType.Text => $"Text(\"{Text}\")",
                RichTextTokenType.OpenTag => $"Open(<{TagName}={TagValue}>)",
                RichTextTokenType.CloseTag => $"Close(</{TagName}>)",
                RichTextTokenType.SelfCloseTag => $"SelfClose(<{TagName}>)",
                RichTextTokenType.LineBreak => "Newline",
                _ => "Unknown"
            };
        }
    }
}
