using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Config
{
    public class PlayerUIConfig
    {
        [Description("Enable or disable common hints. Changing this option might cause error if other plugins are using CommonHints")]
        public bool EnableCommonHints { get; set; } = true;

        [Description("Enable or disable player effects. Changing this option might cause error if other plugins are using Effects")]
        public bool EnableEffects { get; set; } = true;

        [Description("Enable or disable playerUITemplate")]
        public bool EnableUITemplates { get; set; } = true;
    }
}
