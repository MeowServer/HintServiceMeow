using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to get the size of the characters.
    /// </summary>
    internal class FontTool : IFontTool
    {
        public static IFontTool Instance { get; } = new FontTool();

        private const float BaseFontSize = 34.7f;
        private const float DefaultFontWidth = 67.81861f;

        private static readonly ConcurrentDictionary<char, float> ChWidth = new();

        static FontTool()
        {
            ConcurrentTaskDispatcher.Instance.Enqueue(async () =>
            {
                try
                {
                    using Stream infoStream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("HintServiceMeow.TextWidth");
                    using ZipArchive archive = new(infoStream, ZipArchiveMode.Read);
                    using var entryStream = archive.Entries.First(x => x.Name == "TextWidth").Open();
                    using var reader = new StreamReader(entryStream);

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == string.Empty)
                            continue;

                        int sep = line.IndexOf(':');
                        if (sep <= 0)
                            continue;

                        char key = (char)int.Parse(line.Substring(0, sep));
                        float value = float.Parse(line.Substring(sep + 1).TrimStart(), CultureInfo.InvariantCulture);

                        ChWidth[key] = value;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                }
            });
        }

        public float GetCharWidth(char c, float fontSize, TextStyle style)
        {
            if (char.IsControl(c))
                return 0f;

            float ratio = fontSize / BaseFontSize * 1.25f; //1.25 is estimated value

            if ((style & TextStyle.Bold) == TextStyle.Bold)
                ratio *= 1.15f;

            if (!ChWidth.TryGetValue(c, out float width))
                width = DefaultFontWidth;

            return width * ratio;
        }
    }
}
