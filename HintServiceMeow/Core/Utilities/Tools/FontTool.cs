using Exiled.API.Features;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to get the size of the font Hint is using.
    /// </summary>
    internal static class FontTool
    {
        private static readonly float BaseFontSize = 1;
        private static readonly ConcurrentDictionary<char, CharSize> RegularChSize = new ConcurrentDictionary<char, CharSize>();
        private static readonly ConcurrentDictionary<char, CharSize> BoldChSize = new ConcurrentDictionary<char, CharSize>();
        private static readonly ConcurrentDictionary<char, CharSize> ItalicChSize = new ConcurrentDictionary<char, CharSize>();
        private static readonly ConcurrentDictionary<char, CharSize> BoldItalicChSize = new ConcurrentDictionary<char, CharSize>();

        public static void InitializeFont()
        {
            using (var bmp = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.PageUnit = GraphicsUnit.Pixel;

                using (var regularFont = GetFont(BaseFontSize, FontStyle.Regular))
                using (var boldFont = GetFont(BaseFontSize, FontStyle.Bold))
                using (var italicFont = GetFont(BaseFontSize, FontStyle.Italic))
                using (var boldItalicFont = GetFont(BaseFontSize, FontStyle.Bold | FontStyle.Italic))
                {
                    var format = new StringFormat();
                    //format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                    for (int i = char.MinValue; i <= char.MaxValue; i++)
                    {
                        char c = (char)i;
                        string text = c.ToString();

                        var regular = graphics.MeasureString(text, regularFont, PointF.Empty, format);
                        var bold = graphics.MeasureString(text, boldFont, PointF.Empty, format);
                        var italic = graphics.MeasureString(text, italicFont, PointF.Empty, format);
                        var boldItalic = graphics.MeasureString(text, boldItalicFont, PointF.Empty, format);

                        RegularChSize[c] = new CharSize(regular.Width, regular.Height);
                        BoldChSize[c] = new CharSize(bold.Width, bold.Height);
                        ItalicChSize[c] = new CharSize(italic.Width, italic.Height);
                        BoldItalicChSize[c] = new CharSize(boldItalic.Width, boldItalic.Height);
                    }
                }
            }
        }

        public static Font GetFont(float fontSize, FontStyle style)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream fontStream = assembly.GetManifestResourceStream("HintServiceMeow.Roboto-Light"))
            {
                if (fontStream == null)
                {
                    throw new FileNotFoundException("Embedded font resource not found.");
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fontStream.CopyTo(memoryStream);

                    // Load the custom font from the memory stream
                    PrivateFontCollection fontCollection = new PrivateFontCollection();

                    // Allocate memory and copy the font data
                    IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem((int)memoryStream.Length);
                    byte[] fontData = memoryStream.ToArray();
                    System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, (int)memoryStream.Length);

                    // Add the font to the collection
                    fontCollection.AddMemoryFont(fontPtr, (int)memoryStream.Length);

                    // Free the memory
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

                    // Create and return the font object
                    FontFamily fontFamily = fontCollection.Families[0];
                    return new Font(fontFamily, fontSize, style);
                }
            }
        }

        public static CharSize GetCharSize(char c, float fontSize)
        {
            var size = RegularChSize[c];

            return new CharSize(size.Width * fontSize / BaseFontSize, size.Height * fontSize / BaseFontSize);
        }

        public static CharSize GetCharSize(char c, float fontSize, FontStyle style)
        {
            var size = RegularChSize[c];

            switch (style)
            {
                case FontStyle.Bold:
                    size = BoldChSize[c];
                    break;
                case FontStyle.Italic:
                    size = ItalicChSize[c];
                    break;
                case FontStyle.Bold | FontStyle.Italic:
                    size = BoldItalicChSize[c];
                    break;
                default:
                    size = RegularChSize[c];
                    break;
            }

            return new CharSize(size.Width * fontSize / BaseFontSize, size.Height * fontSize / BaseFontSize);
        }

        public struct CharSize
        {
            public float Width;
            public float Height;

            public CharSize(float width, float height)
            {
                Width = width;
                Height = height;
            }

            public override string ToString()
            {
                return $"Width: {Width}, Height: {Height}";
            }
        }
    }
}
