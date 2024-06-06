using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Config
{
    public class GeneralHumanTemplateConfig
    {
        public string TopBar { get; set; } = "TPS:{TPS} | {PlayerName}";
        public string BottomBar { get; set; } = "{PlayerNickname}|<color={RoleColor}>{Role}</color>|{AmmoInfo}|{ArmorInfo}|<color=#7CB342>Meow</color>";
    }

    public class SCPTemplateConfig
    {
        public string TopBar { get; set; } = "TPS:{TPS} | {PlayerName}";
        public string BottomBar { get; set; } = "{PlayerNickname}|<color={RoleColor}>{Role}</color>|剩余{TeammateCount}队友|<color=#7CB342>Meow</color>";
    }

    public class CustomHumanTemplateConfig
    {
        public string TopBar { get; set; } = "TPS:{TPS} | {PlayerName}";
        public string BottomBar { get; set; } = "{PlayerNickname}|<color={RoleColor}>{Role}</color>|{AmmoInfo}|{ArmorInfo}|<color=#7CB342>Meow</color>";
    }

    public class CustomSCPTemplateConfig
    {
        public string TopBar { get; set; } = "TPS:{TPS} | {PlayerName}";
        public string BottomBar { get; set; } = "{PlayerNickname}|<color=#EC2222>{Role}</color>|剩余{TeammateCount}队友|{AmmoInfo}|{ArmorInfo}|<color=#7CB342>Meow</color>";
    }

    public class SpectatorTemplateConfig
    {
        [Description("RespawnTimer: Time interval between each hint")]
        public int HintDisplayInterval { get; set; } = 10;

        [Description("RespawnTimer: A list of hints you want to display")]
        public List<string> Hints { get; private set; } = new List<string>() { "Some hints", "Some other hints" };
    }
}
