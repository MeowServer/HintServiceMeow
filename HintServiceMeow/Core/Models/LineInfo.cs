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

        public float Width { get; }
        public float Height { get; }

        public LineInfo(IReadOnlyList<CharacterInfo> characters, HintAlignment alignment, float lineHeight, bool hasLineHeight, float pos, string rawText)
        {
            Characters = characters ?? throw new ArgumentNullException(nameof(characters), "Characters cannot be null.");

            Alignment = alignment;
            LineHeight = lineHeight;
            HasLineHeight = hasLineHeight;
            Pos = pos;

            RawText = rawText;

            if (Characters.Count == 0)
            {
                Height = 0;
                Width = 0;
            }
            else
            {
                Height = HasLineHeight ? LineHeight : Characters.Max(c => c.Height);
                Width = Characters.Sum(c => c.Width);
            }
        }
    }
}
