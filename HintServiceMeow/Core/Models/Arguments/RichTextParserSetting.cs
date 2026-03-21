using System;
using System.Collections.Generic;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Arguments
{
    internal class RichTextParserSetting : IEquatable<RichTextParserSetting>
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

        public bool Equals(RichTextParserSetting other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Equals(DefaultStyle, other.DefaultStyle)
                && ParameterIndex == other.ParameterIndex
                && SequenceEqual(Parameters, other.Parameters)
                && SequenceEqual(IllegalTags, other.IllegalTags)
                && IgnoreTags.SetEquals(other.IgnoreTags);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RichTextParserSetting);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = (hash * 31) + (DefaultStyle != null ? DefaultStyle.GetHashCode() : 0);
                hash = (hash * 31) + ParameterIndex;

                if (Parameters != null)
                {
                    foreach (var p in Parameters)
                    {
                        hash = (hash * 31) + (p != null ? p.GetHashCode() : 0);
                    }
                }

                if (IllegalTags != null)
                {
                    foreach (var tag in IllegalTags)
                    {
                        hash = (hash * 31) + (tag != null ? tag.GetHashCode() : 0);
                    }
                }

                if (IgnoreTags != null)
                {
                    foreach (var tag in new SortedSet<string>(IgnoreTags))
                    {
                        hash = (hash * 31) + (tag != null ? tag.GetHashCode() : 0);
                    }
                }

                return hash;
            }
        }

        private static bool SequenceEqual<T>(T[] a, T[] b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (!Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }
    }
}
