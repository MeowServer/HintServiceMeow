using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Config
{

    public class PlayerUIToolsConfig
    {
        [Description("Message template when the player has no armor")]
        public string NoArmor { get; set; } = "没有装甲";

        [Description("Message template when the player has no ammo")]
        public string NoAmmo { get; set; } = "没有备弹";

        [Description("Message template when the player has 1 type of ammo")]
        public string AmmoHint1 { get; set; } = "{Ammo}共{NumOfAmmo}发";

        [Description("Message template when the player has multiple types of ammo")]
        public string AmmoHint2 { get; set; } = "各种备弹共{NumOfAmmo}发";
    }
}
