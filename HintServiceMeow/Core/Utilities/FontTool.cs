using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Drawing.Text;

using UnityEngine;
using HintServiceMeow.Core.Models.Hints;

//NW API
using PluginAPI.Core;


namespace HintServiceMeow.Core.Utilities
{
    internal static class FontTool
    {
        private static Dictionary<Tuple<string, int>, float> _textWidthCache = new Dictionary<Tuple<string, int>, float>();

        private static Font _font;

        private static Font Font => _font;

        public static void CheckFontFile()
        {
            string fontName = "Araboto-Light";

            bool isFontInstalled = new InstalledFontCollection().Families.Any(fontFamily => fontFamily.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase));
            if (!isFontInstalled)
            {
                Log.Error("Did not found required font in system.");
                Log.Error("The required font file will be place on the desktop. Please install it manually.");

                CopyEmbeddedFont();
            }

            _font = Font.CreateDynamicFontFromOSFont(fontName, 20);
        }

        private static void CopyEmbeddedFont()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                var resourceName = "HintServiceMeow.FontFile";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException(resourceName + " not found.");
                    }

                    string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Required Font.ttf");
                    using (FileStream tempFile = new FileStream(desktopPath, FileMode.Create))
                    {
                        stream.CopyTo(tempFile);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error loading embedded Font: {e.Message}");
            }
        }

        public static float GetTextWidth(AbstractHint hint)
        {
            return GetTextWidth(hint.Text, hint.FontSize);
        }

        public static float GetTextWidth(string text, int size = 20)
        {
            float width;
            var tuple = Tuple.Create(text, size);

            if (!_textWidthCache.TryGetValue(tuple, out width))
            {
                TextGenerationSettings settings = new TextGenerationSettings()
                {
                    font = Font,
                    fontSize = size,
                    fontStyle = FontStyle.Normal,
                    richText = true,
                    scaleFactor = 2f,
                    color = Color.black,
                    lineSpacing = 1.0f,
                    textAnchor = TextAnchor.MiddleCenter,
                    alignByGeometry = false,
                    resizeTextForBestFit = false,
                    updateBounds = true,
                    verticalOverflow = VerticalWrapMode.Overflow,
                    horizontalOverflow = HorizontalWrapMode.Overflow
                };

                text = GetSmallCapsText(text, size);

                var textGenerator = new TextGenerator();
                textGenerator.Populate(text, settings);
                width = textGenerator.GetPreferredWidth(text, settings);

                _textWidthCache.Add(tuple, width);
            }

            return width;
        }

        private static string GetSmallCapsText(string text, int size)
        {
            if(string.IsNullOrEmpty(text))
                return string.Empty;

            Dictionary<string, string> tagPlaceholders = new Dictionary<string, string>();
            string protectedText = Regex.Replace(text, "<.*?>", m =>
            {
                string placeholder = $"{{TAG{tagPlaceholders.Count}}}";
                tagPlaceholders[placeholder] = m.Value;
                return placeholder;
            });

            // Use regex to find all sequences of lowercase letters and replace them with tagged uppercase letters
            text = Regex.Replace(text, "[a-z]+", (x) => $"<size={size * 16 / 20}>{x.Value.ToUpper()}</size>");

            foreach (var placeholder in tagPlaceholders)
            {
                text = text.Replace(placeholder.Key, placeholder.Value);
            }

            return text;
        }
    }
}
