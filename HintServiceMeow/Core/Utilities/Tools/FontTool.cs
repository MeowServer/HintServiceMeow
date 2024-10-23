using HintServiceMeow.Core.Enum;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream infoStream = assembly.GetManifestResourceStream("HintServiceMeow.TextWidth"))
            {
                StreamReader reader = new StreamReader(infoStream);
                var dict = new DeserializerBuilder().Build().Deserialize<ConcurrentDictionary<int, float>>(reader.ReadToEnd());

                foreach(var kvp in dict)
                {
                    ChWidth.TryAdd((char)kvp.Key, kvp.Value);
                }
            }
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
