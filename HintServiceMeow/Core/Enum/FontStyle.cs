namespace HintServiceMeow.Core.Enum
{
    using System;

    /// <summary>
    /// Specifies the font style applied to text, supporting bold and italic as flags.
    /// </summary>
    [Flags]
    internal enum FontStyle
    {
        Normal = 0x0000,
        Bold = 0x0001,
        Italic = 0x0010,
    }
}
