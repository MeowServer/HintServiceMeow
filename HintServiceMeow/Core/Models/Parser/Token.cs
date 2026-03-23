namespace HintServiceMeow.Core.Models.Parser
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;

    /// <summary>
    /// Represents a single token in the rich text.
    /// </summary>
    internal struct Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> struct.
        /// </summary>
        /// <param name="type">Type of the token.</param>
        /// <param name="tagName">Name of the tag, if this token is a tag.</param>
        /// <param name="tagValue">Value of the tag, if exists.</param>
        /// <param name="text">Text, if this token is a text token.</param>
        /// <param name="parameter">Parameter, if this token is a parameter token.</param>
        public Token(RichTextTokenType type, string? tagName = null, string? tagValue = null, string? text = null, IParameter? parameter = null)
        {
            Type = type;
            TagName = tagName;
            TagValue = tagValue;
            Text = text;
            Parameter = parameter;
        }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
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
        public IParameter? Parameter { get; set; } = null;

        /// <summary>
        /// Gets a text token with the specified text content.
        /// </summary>
        /// <param name="text">Text represented by this token.</param>
        /// <returns>A <see cref="Token"/> instance.</returns>
        public static Token GetText(string text)
        {
            return new Token(RichTextTokenType.Text, text: text);
        }

        /// <summary>
        /// Creates a new <see cref="Token"/> instance with the specified type, tag name, and optional tag value.
        /// </summary>
        /// <param name="type">The type of the rich text token to create.</param>
        /// <param name="tagName">The name of the tag associated with the token. Cannot be null.</param>
        /// <param name="tagValue">The value of the tag, or null if the tag does not have a value.</param>
        /// <returns>A new Token instance initialized with the specified type, tag name, and tag value.</returns>
        public static Token GetTag(RichTextTokenType type, string tagName, string? tagValue)
        {
            return new Token(type, tagName: tagName, tagValue: tagValue);
        }

        /// <summary>
        /// Creates a <see cref="Token"/> instance that represents a line break in rich text content.
        /// </summary>
        /// <returns>A <see cref="Token"/> instance representing a line break.</returns>
        public static Token GetLineBreak()
        {
            return new Token(RichTextTokenType.LineBreak);
        }

        /// <summary>
        /// Creates a token that represents the specified parameter for use in rich text formatting.
        /// </summary>
        /// <param name="parameter">The parameter to be represented as a token. Cannot be null.</param>
        /// <returns>A token of type Parameter that encapsulates the specified parameter.</returns>
        public static Token GetParameter(IParameter parameter)
        {
            return new Token(RichTextTokenType.Parameter, parameter: parameter);
        }

        /// <summary>
        /// Returns a string that represents the current rich text token in a human-readable format.
        /// </summary>
        /// <returns>A string describing the token type and its associated data. The format varies depending on the token type.</returns>
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
