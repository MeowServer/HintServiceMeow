using HintServiceMeow.Core.Enum;

using PluginAPI.Core;

using System;
using System.Collections.Concurrent;
using System.IO;
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

        private static readonly ConcurrentDictionary<char, float> ChWidth = new ConcurrentDictionary<char, float>();

        static FontTool()
        {
            Task.Run(() =>
            {
                try
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    using Stream infoStream = assembly.GetManifestResourceStream("HintServiceMeow.TextWidth");
                    using StreamReader reader = new StreamReader(infoStream);

                    string yamlFile = reader.ReadToEnd();
                    IDeserializer deserializer = new DeserializerBuilder().Build();

                    ConcurrentDictionary<int, float> dictionary = deserializer.Deserialize<ConcurrentDictionary<int, float>>(yamlFile);

                    foreach (var kvp in dictionary)
                    {
                        ChWidth.TryAdd((char)kvp.Key, kvp.Value);
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

            if (!ChWidth.TryGetValue(c, out var width))
                width = DefaultFontWidth;

            return width * ratio;
        }
    }
}
