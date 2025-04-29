using HintServiceMeow.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HintServiceMeow.Core.Models
{
    internal readonly struct LineInfo
    {
        /// <summary>
        /// A list of character info that include all the characters after parsed. Include the line break at the end(if exist).
        /// </summary>
        public IReadOnlyList<CharacterInfo> Characters { get; }
        public HintAlignment Alignment { get; }
        public float LineHeight { get; }
        public bool HasLineHeight { get; }
        public float Pos { get; }

        public string RawText { get; }

        public float Width
        {
            get
            {
                if (Characters is null || !Characters.Any())
                    return 0;

                return Characters.Sum(c => c.Width);
            }
        }
        public float Height
        {
            get
            {
                if (Characters is null || !Characters.Any())
                    return 0;

                if (HasLineHeight)
                {
                    return LineHeight;
                }

                return Characters.Max(c => c.Height);
            }
        }

        public LineInfo(List<CharacterInfo> characters, HintAlignment alignment, float lineHeight, bool hasLineHeight, float pos, string rawText)
        {
            if (characters is null)
                throw new ArgumentNullException(nameof(characters), "Characters cannot be null.");

            Characters = new List<CharacterInfo>(characters).AsReadOnly();

            Alignment = alignment;
            LineHeight = lineHeight;
            HasLineHeight = hasLineHeight;
            Pos = pos;

            RawText = rawText;
        }
    }
}
