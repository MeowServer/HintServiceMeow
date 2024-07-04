using Exiled.API.Interfaces;

using System.ComponentModel;

namespace HintServiceMeow.Config
{
    public class PluginConfig:IConfig
    {
        internal static PluginConfig Instance;

        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Configs for PlayerDisplay. Changing these configs might leads to errors")]
        public PlayerDisplayConfig PlayerDisplayConfig { get; set; } = new PlayerDisplayConfig();

        [Description("The for PlayerUI. PlayerUI contains commonly used hints")]
        public PlayerUIConfig PlayerUIConfig { get; set; } = new PlayerUIConfig();
    }
}
