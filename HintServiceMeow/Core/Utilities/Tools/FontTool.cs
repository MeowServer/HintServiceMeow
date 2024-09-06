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
        public static ConcurrentDictionary<char, SizeF> RegularChSize = new ConcurrentDictionary<char, SizeF>();
        public static ConcurrentDictionary<char, SizeF> BoldChSize = new ConcurrentDictionary<char, SizeF>();
        public static ConcurrentDictionary<char, SizeF> ItalicChSize = new ConcurrentDictionary<char, SizeF>();
        public static ConcurrentDictionary<char, SizeF> BoldItalicChSize = new ConcurrentDictionary<char, SizeF>();

        public static void InitializeFont()
        {
            // Create a bitmap and graphics object for drawing.
            using (var bmp = new Bitmap(1, 1))  // Minimal size since we are not displaying
            using (var graphics = Graphics.FromImage(bmp))
            using(var regularFont = GetFont(1, FontStyle.Regular))
            using(var boldFont = GetFont(1, FontStyle.Bold))
            using(var italicFont = GetFont(1, FontStyle.Italic))
            using(var boldItalicFont = GetFont(1, FontStyle.Bold | FontStyle.Italic))
            {
                for (int i = char.MinValue; i <= char.MaxValue; i++)
                {
                    char c = (char)i;
                    string text = c.ToString();
                    RegularChSize[c] = graphics.MeasureString(text, regularFont);
                    BoldChSize[c] = graphics.MeasureString(text, boldFont);
                    ItalicChSize[c] = graphics.MeasureString(text, italicFont);
                    BoldItalicChSize[c] = graphics.MeasureString(text, boldItalicFont);
                }
            }
        }

        public static Font GetFont(float fontSize, FontStyle style)
        {
            // Get the current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Load the embedded font resource into a stream
            using (Stream fontStream = assembly.GetManifestResourceStream("HintServiceMeow.Roboto-Light"))
            {
                if (fontStream == null)
                {
                    throw new FileNotFoundException("Embedded font resource not found.");
                }

                // Copy the stream to a MemoryStream
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

        public static SizeF GetCharSize(char c, float fontSize)
        {
            var size = RegularChSize[c];

            return new SizeF(size.Width * fontSize, size.Height * fontSize);
        }

        public static SizeF GetCharSize(char c, float fontSize, FontStyle style)
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
            }

            return new SizeF(size.Width * fontSize, size.Height * fontSize);
        }
    }
}
