namespace HintServiceMeow.UI.Models.RichTags
{
    /// <summary>
    /// Represents the color rich text tag <c>&lt;color&gt;</c>.
    /// Changes the color of the enclosed text.
    /// Example (hex): <c>&lt;color=#FF0000&gt;text&lt;/color&gt;</c>.
    /// Example (named): <c>&lt;color="red"&gt;text&lt;/color&gt;</c>.
    /// </summary>
    public sealed class ColorTag : RichTag
    {
        private readonly string value;

        // ── Predefined common colors ──────────────────────────────────────────

        /// <summary>Red. Syntax: <c>&lt;color=#FF0000&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Red = new ColorTag("#FF0000");

        /// <summary>Green. Syntax: <c>&lt;color=#00FF00&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Green = new ColorTag("#00FF00");

        /// <summary>Blue. Syntax: <c>&lt;color=#0000FF&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Blue = new ColorTag("#0000FF");

        /// <summary>White. Syntax: <c>&lt;color=#FFFFFF&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag White = new ColorTag("#FFFFFF");

        /// <summary>Black. Syntax: <c>&lt;color=#000000&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Black = new ColorTag("#000000");

        /// <summary>Yellow. Syntax: <c>&lt;color=#FFFF00&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Yellow = new ColorTag("#FFFF00");

        /// <summary>Orange. Syntax: <c>&lt;color=#FFA500&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Orange = new ColorTag("#FFA500");

        /// <summary>Purple. Syntax: <c>&lt;color=#800080&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Purple = new ColorTag("#800080");

        /// <summary>Cyan. Syntax: <c>&lt;color=#00FFFF&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Cyan = new ColorTag("#00FFFF");

        /// <summary>Magenta. Syntax: <c>&lt;color=#FF00FF&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Magenta = new ColorTag("#FF00FF");

        /// <summary>Grey. Syntax: <c>&lt;color=#808080&gt;text&lt;/color&gt;</c>.</summary>
        public static readonly ColorTag Grey = new ColorTag("#808080");

        private ColorTag(string value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string OpenTag => $"<color={this.value}>";

        /// <inheritdoc/>
        public override string CloseTag => "</color>";

        /// <inheritdoc/>
        internal override int Priority => 200;

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="ColorTag"/> with a custom color value.
        /// </summary>
        /// <param name="value">
        /// The color value. Use a hex string starting with <c>#</c> (e.g., <c>#FF4400</c> or <c>#FF4400AA</c>),
        /// or a named color supported by TextMeshPro (e.g., <c>red</c>, <c>blue</c>).
        /// Hex values produce <c>&lt;color=#RRGGBB&gt;</c>; named values produce <c>&lt;color="name"&gt;</c>.
        /// </param>
        /// <returns>A new <see cref="ColorTag"/> with the specified color value.</returns>
        public static ColorTag Get(string value)
        {
            string formatted = value.StartsWith("#") ? value : $"\"{value}\"";
            return new ColorTag(formatted);
        }

        /// <summary>
        /// Creates a <see cref="ColorTag"/> from individual red, green, and blue byte components.
        /// </summary>
        /// <param name="red">The red component (0–255).</param>
        /// <param name="green">The green component (0–255).</param>
        /// <param name="blue">The blue component (0–255).</param>
        /// <returns>A new <see cref="ColorTag"/> for the specified RGB color.</returns>
        public static ColorTag Get(byte red, byte green, byte blue)
        {
            return new ColorTag($"#{red:X2}{green:X2}{blue:X2}");
        }
    }
}
