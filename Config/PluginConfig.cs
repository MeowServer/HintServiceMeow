using Exiled.API.Enums;
using Exiled.API.Interfaces;
using HintServiceMeow.UITemplates;
using PlayerRoles;
using Respawning;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HintServiceMeow.Config
{
    public class PluginConfig:IConfig
    {
        public static PluginConfig instance;

        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Configs for PlayerDisplay. Changing these configs might leads to errors")]
        public PlayerDisplayConfig PlayerDisplayConfig { get; set; } = new PlayerDisplayConfig();


        [Description("The config for general names")]
        public GeneralConfig GeneralConfig { get; set; } = new GeneralConfig();

        [Description("All of the following configs are used for PlayerUI")]
        public bool EnablePlayerUI { get; set; } = true;

        [Description("The config for PlayerUI")]
        public PlayerUIConfig PlayerUIConfig { get; set; } = new PlayerUIConfig();

        [Description("The config for PlayerUITools, will affect multiple templates")]
        public PlayerUIToolsConfig PlayerUIToolsConfig { get; private set; } = new PlayerUIToolsConfig();

        [Description("The config for general human tempalte")]
        public GeneralHumanTemplateConfig GeneralHumanTemplateConfig { get; private set; } = new GeneralHumanTemplateConfig();

        [Description("The config for scp tempalte")]
        public SCPTemplateConfig ScpTemplateConfig { get; private set; } = new SCPTemplateConfig();

        [Description("The config for custom human tempalte")]
        public CustomHumanTemplateConfig CustomHumanTemplate { get; private set; } = new CustomHumanTemplateConfig();

        [Description("The config for custom scp tempalte")]
        public CustomSCPTemplateConfig CustomSCPTemplate { get; private set; } = new CustomSCPTemplateConfig();

        [Description("The config for spectator tempalte")]
        public SpectatorTemplateConfig SpectatorTemplateConfig { get; private set; } = new SpectatorTemplateConfig();
    }
}
