using System.Collections.Generic;
using System.ComponentModel;

namespace HintServiceMeow
{
    internal class PluginConfig
    {
        public static PluginConfig Instance => Plugin.Config;

        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("By using this feature, it might make plugin that is imcompatible with HintServiceMeow compatible. This is a experimental feature")]
        public bool UseHintCompatibilityAdapter { get; set; } = true;

        [Description("The assembly that you do not want to included in the Compatibility Adapter. Use command GetCompatAssemblyName to get the name of all the assemblies")]
        public List<string> DisabledCompatAdapter { get; set; } = new List<string>();

        [Description("The for CommonHint. CommonHint contains commonly used hints")]
        public PlayerUIConfig PlayerUIConfig { get; set; } = new PlayerUIConfig();
    }
}
