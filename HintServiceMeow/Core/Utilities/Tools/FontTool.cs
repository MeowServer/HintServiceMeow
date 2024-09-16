﻿using HintServiceMeow.Core.Enum;
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
        private static readonly float BaseFontSize = 34.7f;

        private static readonly ConcurrentDictionary<char, float> ChSize = new ConcurrentDictionary<char, float>();

        public static void InitializeFont()
        {
            using (var bmp = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.PageUnit = GraphicsUnit.Pixel;

                using (var regularFont = GetFont(BaseFontSize, FontStyle.Regular))
                {
                    var format = new StringFormat();
                    //format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                    for (int i = char.MinValue; i <= char.MaxValue; i++)
                    {
                        char c = (char)i;
                        string text = c.ToString();

                        ChSize[c] = graphics.MeasureString(text, regularFont, PointF.Empty, format).Width;
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

        public static float GetCharSize(char c, float fontSize, TextStyle style)
        {
            float ratio = 1;

            if ((style & TextStyle.Bold) == TextStyle.Bold)
            {
                ratio = 1.15f;
            }

            if (ChSize.TryGetValue(c, out var width))
            {
                return width * fontSize / BaseFontSize * ratio;
            }

            return 36.52988f / BaseFontSize * ratio;//Default width
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