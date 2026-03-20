using System;
using System.Collections.Generic;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Arguments
{
    internal class RichTextParserSetting
    {
        public RichTextParserSetting(TextMeshStyle defaultStyle,
            Tuple<string, IParameter>[] parameters,
            string[] illegalTags,
            HashSet<string> ignoreTags)
        {
            DefaultStyle = defaultStyle;
            Parameters = parameters;
            IllegalTags = illegalTags;
            IgnoreTags = ignoreTags;
        }

        public TextMeshStyle DefaultStyle { get; }

        public Tuple<string, IParameter>[] Parameters { get; set; }

        public string[] IllegalTags { get; }

        public HashSet<string> IgnoreTags { get; }

        public int ParameterIndex { get; }
    }
}
