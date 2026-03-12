using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Models.Parser
{
    internal struct RichTextToken
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

        public RichTextToken(RichTextTokenType type, string? tagName = null, string? tagValue = null, string? text = null)
        {
            Type = type;
            TagName = tagName;
            TagValue = tagValue;
            Text = text;
        }

        public static RichTextToken GetText(string text)
        {
            return new RichTextToken(RichTextTokenType.Text, text: text);
        }

        public static RichTextToken GetTag(RichTextTokenType type, string tagName, string? tagValue)
        {
            return new RichTextToken(type, tagName: tagName, tagValue: tagValue);
        }

        public static RichTextToken GetLineBreak()
        {
            return new RichTextToken(RichTextTokenType.LineBreak);
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
