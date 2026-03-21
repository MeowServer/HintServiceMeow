namespace HintServiceMeow.Core.Models.Arguments
{
    using HintServiceMeow.Core.Interface;

    public class HintParserResult
    {
        public HintParserResult(string content, IParameter[] parameters)
        {
            Content = content;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the formatted hint content string to be rendered.
        /// </summary>
        public string Content { get; }

        public IParameter[] Parameters { get; }
    }
}
