using System.ComponentModel;

//Exiled
using Exiled.API.Interfaces;

namespace HintService.Config
{
    internal interface IPluginConfig
    {
        bool IsEnabled { get; set; }
        bool Debug { get; set; }

        PlayerUIConfig PlayerUIConfig { get; set; }
    }

    internal class PluginConfig : IPluginConfig
    {
        public static IPluginConfig Instance => Plugin.Config;

        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("The for CommonHint. CommonHint contains commonly used hints")]
        public PlayerUIConfig PlayerUIConfig { get; set; } = new PlayerUIConfig();
    }

    //Exiled Only
    internal class ExiledPluginConfig: IConfig, IPluginConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("The for CommonHint. CommonHint contains commonly used hints")]
        public PlayerUIConfig PlayerUIConfig { get; set; } = new PlayerUIConfig();
    }
}
