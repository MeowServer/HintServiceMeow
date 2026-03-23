namespace HintServiceMeow.Core.Models.Arguments
{
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Parser;

    internal struct RichTextParserResult
    {
        public RichTextParserResult(LineInfo[] lineInfos, IParameter[] parameters, int parameterIndex)
        {
            LineInfos = lineInfos;
            Parameters = parameters;
            ParameterIndex = parameterIndex;
        }

        public LineInfo[] LineInfos { get; }

        public IParameter[] Parameters { get; }

        public int ParameterIndex { get; }
    }
}
