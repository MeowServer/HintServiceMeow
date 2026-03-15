namespace HintServiceMeow.Core.Utilities.Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using HintServiceMeow.Core.Interface;

    /// <summary>
    /// Used to get the size of the characters.
    /// </summary>
    internal class FontTool : IFontTool
    {
        private const float BaseFontSize = 34.7f;
        private const float DefaultFontWidth = 67.81861f;

        private static readonly ConcurrentDictionary<char, float> ChWidth = new();

        static FontTool()
        {
            ConcurrentTaskDispatcher.Instance.Enqueue(() =>
            {
                try
                {
                    using Stream? infoStream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("HintServiceMeow.TextWidth");

                    if (infoStream is null)
                        throw new FileNotFoundException("Could not find text width");

                    using ZipArchive archive = new(infoStream, ZipArchiveMode.Read);
                    using Stream entryStream = archive.Entries.First(x => x.Name == "TextWidth").Open();
                    using StreamReader reader = new(entryStream);

                    string? line;
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

                return Task.CompletedTask;
            });
        }

        public static IFontTool Instance { get; } = new FontTool();

        public float GetCharWidth(char c, float fontSize)
        {
            if (char.IsControl(c))
                return 0f;

            if (!ChWidth.TryGetValue(c, out float width))
                width = DefaultFontWidth;

            return width;
        }
    }
}
