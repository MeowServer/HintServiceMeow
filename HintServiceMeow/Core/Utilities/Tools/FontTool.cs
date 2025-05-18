using HintServiceMeow.Core.Enum;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YamlDotNet.Serialization;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to get the size of the characters.
    /// </summary>
    internal static class FontTool
    {
        private const float BaseFontSize = 34.7f;
        private const float DefaultFontWidth = 67.81861f;

        private static readonly ConcurrentDictionary<char, float> ChWidth = new();

        static FontTool()
        {
            Task.Run(() =>
            {
                try
                {
                    using (Stream infoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HintServiceMeow.TextWidth"))
                    using (ZipArchive archive = new ZipArchive(infoStream, ZipArchiveMode.Read))
                    using (var entryStream = archive.Entries.First(x => x.Name == "TextWidth").Open())
                    using (var reader = new StreamReader(entryStream))
                    {
                        Dictionary<int, float> dictionary = new DeserializerBuilder().Build().Deserialize<Dictionary<int, float>>(reader);

                        foreach (KeyValuePair<int, float> kvp in dictionary)
                        {
                            ChWidth.TryAdd((char)kvp.Key, kvp.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
            });
        }

        public static float GetCharWidth(char c, float fontSize, TextStyle style)
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
