using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Parser;

namespace HintServiceMeow.Core.Models.Arguments
{
    internal struct RichTextParserResult
    {
        public LineInfo[] LineInfos { get; }

        public IParameter[] Parameters { get; }

        public int ParameterIndex { get; }

        public RichTextParserResult(LineInfo[] lineInfos, IParameter[] parameters, int parameterIndex)
        {
            LineInfos = lineInfos;
            Parameters = parameters;
            ParameterIndex = parameterIndex;
        }
    }
}
