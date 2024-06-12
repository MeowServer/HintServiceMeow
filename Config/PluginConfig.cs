using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using Respawning;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HintServiceMeow.Config
{
    public class PluginConfig:IConfig
    {
        internal static PluginConfig instance;

        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Configs for PlayerDisplay. Changing these configs might leads to errors")]
        public PlayerDisplayConfig PlayerDisplayConfig { get; set; } = new PlayerDisplayConfig();
    }
}
