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
        private static readonly Dictionary<Tuple<string, int>, float> _textWidthCache = new Dictionary<Tuple<string, int>, float>();

        private static Font _font;

        private static Font Font => _font;

        public static void LoadFontFile()
        {
            try
            {
                string fontName = "Roboto Light";

                bool isFontInstalled = new InstalledFontCollection().Families.Any(fontFamily =>
                    fontFamily.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase));

                if (!isFontInstalled)
                {
                    var fontFile = Path.Combine(Path.GetTempPath(), "HintServiceMeowRequiredFont");

                    CopyEmbeddedFont(fontFile);

                    if (File.Exists(fontFile))
                        _font = new Font(fontFile);
                    else
                        Log.Error("Error while loading the font file! Cannot find the font file ");

                    if (_font == null || GetTextWidth("abc") == 0) //Check if the font is loaded correctly
                    {
                        _font = null;

                        Log.Warning("Failed to load font file. You can install the font manually. Font: Roboto Light");
                        Log.Warning("A font file will be put onto your server's folder. Please install it and restart the server");
                        Log.Warning("If you cannot install the font, you can ignore this message");

                        var path = Path.Combine(Environment.CurrentDirectory, "Required Font.ttf");
                        CopyEmbeddedFont(path);
                    }
                }
                else
                {
                    _font = Font.CreateDynamicFontFromOSFont(fontName, 20);
                }
            }
            catch(Exception ex)
            {
                Log.Error("Error loading font file：" + ex);
            }
        }

        private static void CopyEmbeddedFont(string path)
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

                    using (FileStream tempFile = new FileStream(path, FileMode.Create))
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
            return GetTextWidth(hint.Content.GetText(), hint.FontSize);
        }

        public static float GetTextWidth(string text, int size = 20)
        {
            float width;

            if (string.IsNullOrEmpty(text))
                return 0;

            var tuple = Tuple.Create(text, size);
            if (_textWidthCache.TryGetValue(tuple, out width))
                return width;

            if (Font != null)
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
            }
            else
            {
                string noTagText = Regex.Replace(text, "<.*?>", string.Empty);
                string longestText = noTagText.Split('\n').OrderByDescending(x => x.Length).First();
                width = longestText.Length * size;
            }

            _textWidthCache.Add(tuple, width);
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
