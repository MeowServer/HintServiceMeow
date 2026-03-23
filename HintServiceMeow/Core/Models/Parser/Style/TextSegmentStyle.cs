namespace HintServiceMeow.Core.Models.Parser.Style
{
    using System;
    using HintServiceMeow.Core.Models.Parser.ValueObject;

    internal class TextSegmentStyle
    {
        public TextSegmentStyle(
            float fontSize,
            Color color,
            float? alpha,
            bool bold,
            bool italic,
            bool underline,
            bool strikethrough,
            int superscript,
            int subscript,
            float? vOffset,
            float? rotate,
            float? charSpace,
            float? monospace,
            Color? mark,
            string? font,
            int? fontWeight)
        {
            FontSize = fontSize;
            Color = color;
            Alpha = alpha;
            Bold = bold;
            Italic = italic;
            Underline = underline;
            Strikethrough = strikethrough;
            Superscript = superscript;
            Subscript = subscript;
            VOffset = vOffset;
            Rotate = rotate;
            CharSpace = charSpace;
            Monospace = monospace;
            Mark = mark;
            Font = font;
            FontWeight = fontWeight;
        }

        public static TextSegmentStyle Default => new TextSegmentStyle(
            fontSize: 40,
            color: new Color(255, 255, 255),
            alpha: null,
            bold: false,
            italic: false,
            underline: false,
            strikethrough: false,
            superscript: 0,
            subscript: 0,
            vOffset: null,
            rotate: null,
            charSpace: null,
            monospace: null,
            mark: null,
            font: null,
            fontWeight: null);

        #region Data
        public float FontSize { get; set; }

        public Color Color { get; set; }

        public float? Alpha { get; set; }

        public bool Bold { get; set; }

        public bool Italic { get; set; }

        public bool Underline { get; set; }

        public bool Strikethrough { get; set; }

        public int Superscript { get; set; }

        public int Subscript { get; set; }

        public float? VOffset { get; set; }

        public float? Rotate { get; set; }

        public float? CharSpace { get; set; }

        public float? Monospace { get; set; }

        public Color? Mark { get; set; }

        public string? Font { get; set; }

        public int? FontWeight { get; set; }
        #endregion

        public float GetWidth(float totalWidthWithFontSize, int count)
        {
            if (Monospace.HasValue)
                return (Monospace.Value + (CharSpace ?? 0)) * count;

            totalWidthWithFontSize *= (float)Math.Pow(0.5, Superscript + Subscript);
            totalWidthWithFontSize += CharSpace ?? 0;

            return totalWidthWithFontSize;
        }

        public float GetHeight() // TODO: Verify the calculation of this height.
        {
            float height = FontSize;
            height *= (float)Math.Pow(0.5, Superscript + Subscript);
            return height;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not TextSegmentStyle other)
                return false;

            return FontSize == other.FontSize
                && Color.Equals(other.Color)
                && Alpha == other.Alpha
                && Bold == other.Bold
                && Italic == other.Italic
                && Underline == other.Underline
                && Strikethrough == other.Strikethrough
                && Superscript == other.Superscript
                && Subscript == other.Subscript
                && VOffset == other.VOffset
                && Rotate == other.Rotate
                && CharSpace == other.CharSpace
                && Monospace == other.Monospace
                && Nullable.Equals(Mark, other.Mark)
                && Font == other.Font
                && FontWeight == other.FontWeight;
        }
    }
}
