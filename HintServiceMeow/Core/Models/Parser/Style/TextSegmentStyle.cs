namespace HintServiceMeow.Core.Models.Parser.Style
{
    using System;
    using HintServiceMeow.Core.Models.Parser.ValueObject;

    /// <summary>
    /// Represents the style of a text segment, corresponding to the combined state
    /// of various Unity Rich Text tags applied to a portion of text.
    /// </summary>
    internal class TextSegmentStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextSegmentStyle"/> class.
        /// </summary>
        /// <param name="fontSize">The font size in pixels.</param>
        /// <param name="color">The text color.</param>
        /// <param name="alpha">
        /// The text opacity, ranging from 0.0 (fully transparent) to 1.0 (fully opaque).
        /// <see langword="null"/> indicates the value is not explicitly set via a tag,
        /// and the alpha channel of <paramref name="color"/> will be used instead.
        /// </param>
        /// <param name="bold">Whether bold styling is enabled, corresponding to the <c>&lt;b&gt;</c> tag.</param>
        /// <param name="italic">Whether italic styling is enabled, corresponding to the <c>&lt;i&gt;</c> tag.</param>
        /// <param name="underline">Whether underline styling is enabled, corresponding to the <c>&lt;u&gt;</c> tag.</param>
        /// <param name="strikethrough">Whether strikethrough styling is enabled, corresponding to the <c>&lt;s&gt;</c> tag.</param>
        /// <param name="superscript">
        /// The number of nested superscript levels, corresponding to the <c>&lt;sup&gt;</c> tag.
        /// Each level scales the font size and height down to 0.5x.
        /// </param>
        /// <param name="subscript">
        /// The number of nested subscript levels, corresponding to the <c>&lt;sub&gt;</c> tag.
        /// Each level scales the font size and height down to 0.5x.
        /// </param>
        /// <param name="vOffset">
        /// The vertical offset in pixels, corresponding to the <c>&lt;voffset&gt;</c> tag.
        /// <see langword="null"/> indicates no offset is applied.
        /// </param>
        /// <param name="rotate">
        /// The rotation angle in degrees (clockwise positive), corresponding to the <c>&lt;rotate&gt;</c> tag.
        /// <see langword="null"/> indicates no rotation is applied.
        /// </param>
        /// <param name="charSpace">
        /// The extra spacing between characters in pixels, corresponding to the <c>&lt;cspace&gt;</c> tag.
        /// <see langword="null"/> indicates the font's default spacing is used.
        /// </param>
        /// <param name="monospace">
        /// The fixed character width in pixels for monospace rendering, corresponding to the <c>&lt;mspace&gt;</c> tag.
        /// When not <see langword="null"/>, every character is rendered at this fixed width,
        /// with <paramref name="charSpace"/> still added on top.
        /// <see langword="null"/> indicates the font's natural character widths are used.
        /// </param>
        /// <param name="mark">
        /// The highlight background color, corresponding to the <c>&lt;mark&gt;</c> tag.
        /// <see langword="null"/> indicates no highlight is applied.
        /// </param>
        /// <param name="font">
        /// The font name, corresponding to the <c>&lt;font&gt;</c> tag.
        /// <see langword="null"/> indicates the game's default font is used.
        /// </param>
        /// <param name="fontWeight">
        /// The font weight, corresponding to the <c>&lt;font-weight&gt;</c> tag.
        /// <see langword="null"/> indicates the font's default weight is used.
        /// </param>
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

        /// <summary>
        /// Gets the default text segment style for the SCP:SL game environment.
        /// <para>
        /// Defaults: font size 40, solid white, no alpha override, no bold/italic/underline/strikethrough,
        /// no superscript/subscript, no offset/rotation/char-spacing/monospace/highlight,
        /// and the game's default font and weight.
        /// </para>
        /// </summary>
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

        /// <summary>
        /// Gets or sets the font size in pixels.
        /// Corresponds to the Unity Rich Text <c>&lt;size&gt;</c> tag.
        /// </summary>
        public float FontSize { get; set; }

        /// <summary>
        /// Gets or sets the text color.
        /// Corresponds to the Unity Rich Text <c>&lt;color&gt;</c> tag.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the text opacity, ranging from 0.0 (fully transparent) to 1.0 (fully opaque).
        /// Corresponds to the Unity Rich Text <c>&lt;alpha&gt;</c> tag.
        /// When <see langword="null"/>, the alpha channel of <see cref="Color"/> is used as a fallback.
        /// </summary>
        public float? Alpha { get; set; }

        /// <summary>
        /// Gets or sets a value indicating  whether bold styling is enabled.
        /// Corresponds to the Unity Rich Text <c>&lt;b&gt;</c> tag.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating  whether italic styling is enabled.
        /// Corresponds to the Unity Rich Text <c>&lt;i&gt;</c> tag.
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether underline styling is enabled.
        /// Corresponds to the Unity Rich Text <c>&lt;u&gt;</c> tag.
        /// </summary>
        public bool Underline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether strikethrough styling is enabled.
        /// Corresponds to the Unity Rich Text <c>&lt;s&gt;</c> tag.
        /// </summary>
        public bool Strikethrough { get; set; }

        /// <summary>
        /// Gets or sets the number of nested superscript levels.
        /// Corresponds to the Unity Rich Text <c>&lt;sup&gt;</c> tag.
        /// Each additional level scales the rendered font size and height down to 0.5x.
        /// </summary>
        public int Superscript { get; set; }

        /// <summary>
        /// Gets or sets the number of nested subscript levels.
        /// Corresponds to the Unity Rich Text <c>&lt;sub&gt;</c> tag.
        /// Each additional level scales the rendered font size and height down to 0.5x.
        /// </summary>
        public int Subscript { get; set; }

        /// <summary>
        /// Gets or sets the vertical offset in pixels.
        /// Corresponds to the Unity Rich Text <c>&lt;voffset&gt;</c> tag.
        /// <see langword="null"/> indicates no additional vertical offset is applied.
        /// </summary>
        public float? VOffset { get; set; }

        /// <summary>
        /// Gets or sets the text rotation angle in degrees (clockwise positive).
        /// Corresponds to the Unity Rich Text <c>&lt;rotate&gt;</c> tag.
        /// <see langword="null"/> indicates no rotation is applied.
        /// </summary>
        public float? Rotate { get; set; }

        /// <summary>
        /// Gets or sets the extra spacing between characters in pixels.
        /// Corresponds to the Unity Rich Text <c>&lt;cspace&gt;</c> tag.
        /// <see langword="null"/> indicates the font's default character spacing is used.
        /// </summary>
        public float? CharSpace { get; set; }

        /// <summary>
        /// Gets or sets the fixed character width in pixels for monospace rendering.
        /// Corresponds to the Unity Rich Text <c>&lt;mspace&gt;</c> tag.
        /// When not <see langword="null"/>, every character is rendered at this fixed width,
        /// with <see cref="CharSpace"/> still added on top of each character.
        /// <see langword="null"/> indicates the font's natural character widths are used.
        /// </summary>
        public float? Monospace { get; set; }

        /// <summary>
        /// Gets or sets the highlight background color behind the text.
        /// Corresponds to the Unity Rich Text <c>&lt;mark&gt;</c> tag.
        /// <see langword="null"/> indicates no highlight is applied.
        /// </summary>
        public Color? Mark { get; set; }

        /// <summary>
        /// Gets or sets the font name.
        /// Corresponds to the Unity Rich Text <c>&lt;font&gt;</c> tag.
        /// <see langword="null"/> indicates the game's default font is used.
        /// </summary>
        public string? Font { get; set; }

        /// <summary>
        /// Gets or sets the font weight.
        /// Corresponds to the Unity Rich Text <c>&lt;font-weight&gt;</c> tag.
        /// <see langword="null"/> indicates the font's default weight is used.
        /// </summary>
        public int? FontWeight { get; set; }

        #endregion

        /// <summary>
        /// Calculates the actual rendered width of this text segment based on the given
        /// total character width and character count.
        /// <para>
        /// If monospace mode is active (<see cref="Monospace"/> is not <see langword="null"/>),
        /// each character is assigned a fixed width of <c>Monospace + CharSpace</c>,
        /// and the result is multiplied by <paramref name="count"/>.
        /// Otherwise, <paramref name="totalWidthWithFontSize"/> is scaled down by the combined
        /// superscript and subscript levels, then <see cref="CharSpace"/> is added.
        /// </para>
        /// </summary>
        /// <param name="totalWidthWithFontSize">
        /// The raw total width of all characters in this segment at the current font size, in pixels.
        /// </param>
        /// <param name="count">
        /// The number of characters in this segment. Only used in monospace mode.
        /// </param>
        /// <returns>The style-adjusted rendered width of the segment, in pixels.</returns>
        public float GetWidth(float totalWidthWithFontSize, int count)
        {
            if (Monospace.HasValue)
                return (Monospace.Value + (CharSpace ?? 0)) * count;

            totalWidthWithFontSize *= (float)Math.Pow(0.5, Superscript + Subscript);
            totalWidthWithFontSize += CharSpace ?? 0;

            return totalWidthWithFontSize;
        }

        /// <summary>
        /// Calculates the actual rendered height of this text segment.
        /// <para>
        /// The height starts from <see cref="FontSize"/> and is scaled down by 0.5x for each
        /// combined level of <see cref="Superscript"/> and <see cref="Subscript"/>.
        /// </para>
        /// </summary>
        /// <returns>The style-adjusted rendered height of the segment, in pixels.</returns>
        /// <remarks>
        /// TODO: The current calculation has not yet been verified against the game's actual rendering behavior.
        /// </remarks>
        public float GetHeight() // TODO: Verify the calculation of this height.
        {
            float height = FontSize;
            height *= (float)Math.Pow(0.5, Superscript + Subscript);
            return height;
        }

        /// <summary>
        /// Determines whether the current style is equal to another <see cref="TextSegmentStyle"/> instance.
        /// All properties participate in the comparison; any differing property causes the method to return <see langword="false"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="TextSegmentStyle"/>
        /// with all properties equal to those of the current instance; otherwise <see langword="false"/>.
        /// </returns>
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