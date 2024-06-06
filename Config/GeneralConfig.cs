using Exiled.API.Enums;
using PlayerRoles;
using Respawning;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Config
{
    public class GeneralConfig
    {
        [Description("Translation of different role types")]
        public Dictionary<RoleTypeId, string> RoleName { get; private set; } = new Dictionary<RoleTypeId, string>()
        {
            { RoleTypeId.None, "无角色"},
            { RoleTypeId.Scp173, "Scp173"},
            { RoleTypeId.ClassD, "D级人员"},
            { RoleTypeId.Spectator, "观察者"},
            { RoleTypeId.Scp106, "Scp106"},
            { RoleTypeId.NtfSpecialist, "九尾狐收容专家"},
            { RoleTypeId.Scp049, "Scp049"},
            { RoleTypeId.Scientist, "科学家"},
            { RoleTypeId.Scp079, "Scp079"},
            { RoleTypeId.ChaosConscript, "混沌征召兵"},
            { RoleTypeId.Scp096, "Scp096"},
            { RoleTypeId.Scp0492, "Scp049-2"},
            { RoleTypeId.NtfSergeant, "九尾狐中士"},
            { RoleTypeId.NtfCaptain, "九尾狐指挥官"},
            { RoleTypeId.NtfPrivate, "九尾狐列兵"},
            { RoleTypeId.Tutorial, "教程人员"},
            { RoleTypeId.FacilityGuard, "设施警卫"},
            { RoleTypeId.Scp939, "Scp939"},
            { RoleTypeId.CustomRole, "自定义角色"},
            { RoleTypeId.ChaosRifleman, "混沌步枪兵"},
            { RoleTypeId.ChaosMarauder, "混沌掠夺者"},
            { RoleTypeId.ChaosRepressor, "混沌压制者"},
            { RoleTypeId.Overwatch, "角色Overwatch"},
            { RoleTypeId.Filmmaker, "摄像机"},
            { RoleTypeId.Scp3114, "Scp3114"},
        };

        [Description("Translation of different team types")]
        public Dictionary<Team, string> TeamName { get; private set; } = new Dictionary<Team, string>()
        {
            { Team.ChaosInsurgency, "混沌分裂者" },
            { Team.ClassD, "D级人员" },
            { Team.Dead, "死后阵营" },
            { Team.FoundationForces, "基金会阵营" },
            { Team.OtherAlive, "其他阵营" },
            { Team.Scientists, "科学家" },
            { Team.SCPs, "SCP"},
        };

        [Description("Translation of different item types")]
        public Dictionary<ItemType, string> ItemName { get; private set; } = new Dictionary<ItemType, string>()
        {
            {ItemType.KeycardJanitor,"保洁员钥匙卡" },
            {ItemType.KeycardScientist,"科学家钥匙卡" },
            {ItemType.KeycardResearchCoordinator,"研究主管钥匙卡" },
            {ItemType.KeycardZoneManager,"区域主管钥匙卡" },
            {ItemType.KeycardGuard,"设施警卫钥匙卡"},
            {ItemType.KeycardMTFOperative,"九尾狐特工钥匙卡" },
            {ItemType.KeycardMTFCaptain,"九尾狐指挥官钥匙卡" },
            {ItemType.KeycardFacilityManager,"设施主管钥匙卡" },
            {ItemType.KeycardChaosInsurgency,"混沌破解装置" },
            {ItemType.KeycardO5,"O5钥匙卡" },
            {ItemType.Radio,"对讲机" },
            {ItemType.GunCOM15,"COM15" },
            {ItemType.Medkit,"急救包" },
            {ItemType.Flashlight,"手电筒" },
            {ItemType.MicroHID,"MicroHID" },
            {ItemType.SCP500,"SCP500" },
            {ItemType.SCP207,"SCP207" },
            {ItemType.Ammo12gauge,"12口径弹药" },
            {ItemType.GunE11SR,"E11SR" },
            {ItemType.GunCrossvec,"Crossvec" },
            {ItemType.Ammo556x45,"556x45弹药" },
            {ItemType.GunFSP9,"FSP9" },
            {ItemType.GunLogicer,"Logicer" },
            {ItemType.GrenadeHE,"高爆手雷" },
            {ItemType.GrenadeFlash,"闪光弹" },
            {ItemType.Ammo44cal,"44cal弹药" },
            {ItemType.Ammo762x39,"762x39弹药" },
            {ItemType.Ammo9x19,"9x19弹药" },
            {ItemType.GunCOM18,"COM18" },
            {ItemType.SCP018,"SCP018" },
            {ItemType.SCP268,"SCP268" },
            {ItemType.Adrenaline,"肾上腺素" },
            {ItemType.Painkillers,"止痛药" },
            {ItemType.Coin,"硬币" },
            {ItemType.ArmorLight,"轻型装甲" },
            {ItemType.ArmorCombat,"中型装甲" },
            {ItemType.ArmorHeavy,"重型装甲" },
            {ItemType.GunRevolver,"Revolver" },
            {ItemType.GunAK,"AK" },
            {ItemType.GunShotgun,"Shotgun" },
            {ItemType.SCP330,"SCP330" },
            {ItemType.SCP2176,"SCP2176" },
            {ItemType.SCP244a,"SCP244a" },
            {ItemType.SCP244b,"SCP244b" },
            {ItemType.AntiSCP207,"粉色SCP207" }
        };

        [Description("Translation of different zone names")]
        public Dictionary<ZoneType, string> ZoneName { get; private set; } = new Dictionary<ZoneType, string>()
        {
            {ZoneType.LightContainment, "轻收容区" },
            {ZoneType.HeavyContainment, "重收容区" },
            {ZoneType.Entrance, "入口区" },
            {ZoneType.Surface, "地表区" },
            {ZoneType.Unspecified, "未指定区域" }
        };

        [Description("Translation of different AmmoName types")]
        public Dictionary<AmmoType, string> AmmoName { get; private set; } = new Dictionary<AmmoType, string>()
        {
            {AmmoType.Ammo12Gauge,"12口径弹药" },
            {AmmoType.Nato556,"556x45弹药" },
            {AmmoType.Ammo44Cal,"44cal弹药" },
            {AmmoType.Nato762,"762x39弹药" },
            {AmmoType.Nato9,"9x19弹药" },
            {AmmoType.None,"无弹药" }
        };

        [Description("Translation of different respawnable role types")]
        public Dictionary<SpawnableTeamType, string> RespawnTeamDictionary { get; private set; } = new Dictionary<SpawnableTeamType, string>
        {
            {SpawnableTeamType.NineTailedFox,"<color=#0096FF>九尾狐</color>" },
            {SpawnableTeamType.ChaosInsurgency,"<color=#0D7D35>混沌分裂者</color>" },
            {SpawnableTeamType.None, "暂时未定" }
        };

        [Description("Translation of different warhead status")]
        public Dictionary<WarheadStatus, string> WarheadStatusDictionary { get; private set; } = new Dictionary<Exiled.API.Enums.WarheadStatus, string>
        {
            {WarheadStatus.Armed, "<color=#d0652f>已就绪</color>" },
            {WarheadStatus.NotArmed, "<color=#7fd827>未就绪</color>" },
            {WarheadStatus.Detonated, "<color=#ce313f>已被引爆</color>" },
            {WarheadStatus.InProgress, "<color=#d0652f>正在倒计时</color>" },
        };
    }
}
