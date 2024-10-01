using HintServiceMeow.Core.Enum;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using PluginAPI.Core;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to get the size of the characters.
    /// </summary>
    internal static class FontTool
    {
        private const float BaseFontSize = 34.7f;
        private const float DefaultFontWidth = 36.52988f;

        private static readonly ConcurrentDictionary<char, float> ChWidth = new ConcurrentDictionary<char, float>();

        static FontTool()
        {
            //Initialize ch width
            Task.Run(() =>
            {
                try
                {
                    using (var bmp = new Bitmap(1, 1))
                    using (var graphics = Graphics.FromImage(bmp))
                    using (var regularFont = GetFont(BaseFontSize, FontStyle.Regular))
                    {
                        graphics.PageUnit = GraphicsUnit.Pixel;

                        for (int i = char.MinValue; i <= char.MaxValue; i++)
                        {
                            char c = (char)i;

                            float width = DefaultFontWidth;
                            try
                            {
                                width = graphics.MeasureString(c.ToString(), regularFont).Width;
                            }
                            catch (Exception) { }//Do not handle error since some system might not support certain character

                            if (char.IsControl(c))// 0 width for control characters like \n
                                width = 0;

                            if (width.Equals(DefaultFontWidth))
                                continue;

                            ChWidth[c] = width;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Warning("This error can be ignored");
                    Log.Warning($"Failed to initialize font tool: {ex}");
                }
            });
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

        public static float GetCharWidth(char c, float fontSize, TextStyle style)
        {
            float ratio = 1;

            if ((style & TextStyle.Bold) == TextStyle.Bold)
                ratio = 1.15f;

            if (!ChWidth.TryGetValue(c, out var width))
                width = DefaultFontWidth;

            return width * ratio * fontSize / BaseFontSize;
        }
    }
}
