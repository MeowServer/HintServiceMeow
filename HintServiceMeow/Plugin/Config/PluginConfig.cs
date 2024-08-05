using System.ComponentModel;

//Exiled
using Exiled.API.Interfaces;

namespace HintServiceMeow
{
    internal class PluginConfig
    {
        public static PluginConfig Instance => Plugin.Config;

        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("The for CommonHint. CommonHint contains commonly used hints")]
        public PlayerUIConfig PlayerUIConfig { get; set; } = new PlayerUIConfig();
    }

    //Exiled Only
    internal class ExiledPluginConfig: PluginConfig, IConfig
    {
    }
}
