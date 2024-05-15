using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps;
using PlayerRoles;

using Exiled.CustomRoles.API.Features;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Extensions;
using Exiled.API.Features.Roles;

using System;
using System.Collections.Generic;
using System.Linq;

namespace HintServiceMeow
{
    public abstract class PlayerUITemplateBase
    {
        public enum PlayerUITemplateType
        {
            GeneralHuman,
            Spectator,
            SCP,
            CustomSCP,
            CustomHuman
        }
        public abstract PlayerUITemplateType type { get; }

        public Player player;
        public PlayerDisplay playerDisplay;

        public PlayerUITemplateBase(Player player)
        {
            this.player = player;
            this.playerDisplay = PlayerDisplay.Get(player);

            SetUpTemplate();
        }

        public abstract void UpdateTemplate();

        public abstract void SetUpTemplate();

        public abstract void DestructTemplate();
    }

    public abstract class AliveTemplate : PlayerUITemplateBase
    {
        //Top Bar
        public Hint TopBar = new Hint(0, HintAlignment.Center, "", "TopBar1", true).setFontSize(15);

        //Bottom Bar
        public Hint BottomBar = new Hint(710, HintAlignment.Center, "", "BottomBar1", true).setFontSize(20);

        public DynamicHint[] spectatorHints = new DynamicHint[5]
        {
            new DynamicHint(100, 300, HintAlignment.Right, "", "SpectatorHints1", true).setFontSize(20),
            new DynamicHint(120, 320, HintAlignment.Right, "", "SpectatorHints2", true).setFontSize(20),
            new DynamicHint(140, 340, HintAlignment.Right, "", "SpectatorHints3", true).setFontSize(20),
            new DynamicHint(160, 360, HintAlignment.Right, "", "SpectatorHints4", true).setFontSize(20),
            new DynamicHint(180, 380, HintAlignment.Right, "", "SpectatorHints5", true).setFontSize(20)
        };

        public AliveTemplate(Player player) : base(player)
        {
        }

        public override void UpdateTemplate()
        {
            UpdateTopBar();
            UpdateBottomBar();
            UpdateSpectatorHints();
        }

        public override void SetUpTemplate()
        {
            playerDisplay.AddHint(TopBar);
            playerDisplay.AddHint(BottomBar);

            playerDisplay.AddHints(spectatorHints);
        }

        public override void DestructTemplate()
        {
            playerDisplay.RemoveHint(TopBar);
            playerDisplay.RemoveHint(BottomBar);

            playerDisplay.RemoveHints(spectatorHints);
        }

        protected abstract void UpdateTopBar();

        protected abstract void UpdateBottomBar();

        protected virtual void UpdateSpectatorHints()
        {
            List<Player> spectatingPlayers = new List<Player>();

            foreach(Player player in Player.List.ToList().FindAll(x => x.Role.Type == RoleTypeId.Spectator))
            {
                SpectatorRole spectator = player.Role.As<SpectatorRole>();

                if (spectator.SpectatedPlayer == this.player)
                {
                    spectatingPlayers.Add(player);
                }
            }

            spectatorHints[0].message = spectatingPlayers.Count > 0 ? "👥观察者" : string.Empty;
            for (int i = 0; i < 4; i++)
            {
                if(i >= spectatingPlayers.Count)
                    break;

                spectatorHints[i].message = $"-{spectatingPlayers[i]?.Nickname}";
            }
            spectatorHints[4].message = spectatingPlayers.Count > 4 ? $"共{spectatingPlayers.Count}个观察者正在观察您" : string.Empty;

            foreach (DynamicHint hint in spectatorHints)
            {
                if(hint.message != string.Empty)
                {
                    hint.hide = false;
                }
                else
                {
                    hint.hide = true;
                }
            }
        }
    }

    public class GeneralHumanTemplate : AliveTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.GeneralHuman;

        public GeneralHumanTemplate(Player player) : base(player)
        {

        }

        protected override void UpdateTopBar()
        {
            TopBar.hide = false;
            TopBar.message = $"TPS:{Server.Tps} | {player.Nickname}";
        }

        protected override void UpdateBottomBar()
        {
            BottomBar.hide = false;
            BottomBar.message = $"{player.DisplayNickname}|<color={player.Role.Color.ToHex()}>{Plugin.instance.Config.RoleName[player.Role.Type]}</color>|{UICommonTools.GetAmmoInfo(player)}|{UICommonTools.GetArmorInfo(player)}|<color=#7CB342>Meow</color>";
        }
    }

    public class SpectatorTemplate : PlayerUITemplateBase
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.Spectator;

        private Hint topInformation = new Hint(0, HintAlignment.Center, "", null, false).setFontSize(10);

        private Hint respawnTimer = new Hint(420, HintAlignment.Center, "", null, false).setFontSize(25);

        private Hint warheadStatus = new Hint(450, HintAlignment.Left, "", null, false);
        private Hint generatorsStatus = new Hint(470, HintAlignment.Left, "", null, false);

        private Hint spectatorsInformation = new Hint(490, HintAlignment.Right, "", null, false);

        private Hint NTFTicketInformation = new Hint(510, HintAlignment.Right, "", null, false);
        private Hint CITicketInformation = new Hint(530, HintAlignment.Right, "", null, false);

        private Hint hint = new Hint(550, HintAlignment.Center, "", null, false);


        public SpectatorTemplate(Player player) : base(player)
        {

        }

        public override void DestructTemplate()
        {
            playerDisplay.RemoveHint(topInformation);

            playerDisplay.RemoveHint(respawnTimer);

            playerDisplay.RemoveHint(warheadStatus);
            playerDisplay.RemoveHint(generatorsStatus);

            playerDisplay.RemoveHint(spectatorsInformation);

            playerDisplay.RemoveHint(NTFTicketInformation);
            playerDisplay.RemoveHint(CITicketInformation);

            playerDisplay.RemoveHint(hint);
        }

        public override void SetUpTemplate()
        {
            playerDisplay.AddHint(topInformation);

            playerDisplay.AddHint(respawnTimer);

            playerDisplay.AddHint(warheadStatus);
            playerDisplay.AddHint(generatorsStatus);

            playerDisplay.AddHint(spectatorsInformation);

            playerDisplay.AddHint(NTFTicketInformation);
            playerDisplay.AddHint(CITicketInformation);

            playerDisplay.AddHint(hint);
        }

        public override void UpdateTemplate()
        {
            topInformation.message = SpectatorUITools.GetTopInformation();

            respawnTimer.message = SpectatorUITools.GetRespawnTimer();

            warheadStatus.message = SpectatorUITools.GetWarheadStatus();
            generatorsStatus.message = SpectatorUITools.GetGeneratorsStatus();

            spectatorsInformation.message = SpectatorUITools.GetSpectatorsInformation();

            NTFTicketInformation.message = SpectatorUITools.GetNTFTicketInformation();
            CITicketInformation.message = SpectatorUITools.GetCITicketInformation();

            hint.message = SpectatorUITools.GetHint();
        }

        public static class SpectatorUITools
        {
            public static string GetTopInformation()
            {
                //<size=50%>Round time: {round_minutes}:{round_seconds} | TPS: {tps}/{tickrate}</size>
                string template = "游戏已进行{RoundMinutes} : {RoundSecond} | TPS: {TPS}/{tickrate}     ";
                string message = String.Empty;

                message = template
                    .Replace("{RoundMinutes}", Round.ElapsedTime.ToString("mm"))//Time
                    .Replace("{RoundSecond}", Round.ElapsedTime.ToString("ss"))//Time
                    .Replace("{TPS}", Server.Tps.ToString())//TPS
                    .Replace("{tickrate}", "60");//Default TPS: 60

                return message;

            }

            public static string GetRespawnTimer()
            {
                string message = String.Empty;

                if (Respawn.IsSpawning)
                {
                    string template = "您会在{RespawnMinute} : {RespawnSecond}后部署为：{RespawnTeam}";

                    message = template
                    .Replace("{RespawnMinute}", Respawn.TimeUntilSpawnWave.ToString("mm"))//Respawn Time
                    .Replace("{RespawnSecond}", Respawn.TimeUntilSpawnWave.ToString("ss"))//Respawn Time
                    .Replace("{RespawnTeam}", Plugin.instance.Config.RespawnTeamDictionary[Respawn.NextKnownTeam]);//Team
                }
                else
                {
                    string template = "<b>您会在{RespawnMinute} : {RespawnSecond}后重新部署</b>";

                    message = template
                    .Replace("{RespawnMinute}", Respawn.TimeUntilSpawnWave.ToString("mm"))//Respawn Time
                    .Replace("{RespawnSecond}", Respawn.TimeUntilSpawnWave.ToString("ss"));//Respawn Time
                }
                return message;
            }

            public static string GetWarheadStatus()
            {
                //<align=left>Warhead Status: {warhead_status}
                string template = "Alpha核弹状态: {WarheadStatus} {TimeRemaining}";
                string message = String.Empty;


                if (Warhead.Status == Exiled.API.Enums.WarheadStatus.InProgress)
                {
                    message = template
                    .Replace("{WarheadStatus}", Plugin.instance.Config.WarheadStatusDictionary[Warhead.Status])
                    .Replace("{TimeRemaining}", "<color=#d0652f>" + ((int)Warhead.DetonationTimer).ToString() + "秒" + "</color>");
                }
                else
                {
                    message = template
                    .Replace("{WarheadStatus}", Plugin.instance.Config.WarheadStatusDictionary[Warhead.Status])
                    .Replace("{TimeRemaining}", String.Empty);
                }

                return message;

            }

            public static string GetGeneratorsStatus()
            {
                //Generators: {generator_engaged}/{generator_count}

                string templateWhenNotFullyActivated = "{EngagedGeneratorNum}个发电机被激活 共有{TotalGeneratorNum}个发电机";
                string templateWhenFullyActivated = "所有发电机已被激活";
                string message = String.Empty;

                var activatedGeneratorNum = Scp079Recontainer.AllGenerators.Count(x => x.Engaged);
                var totalGeneratorNum = Scp079Recontainer.AllGenerators.Count();

                if (activatedGeneratorNum < totalGeneratorNum)
                {
                    message = templateWhenNotFullyActivated
                    .Replace("{EngagedGeneratorNum}", activatedGeneratorNum.ToString())
                    .Replace("{TotalGeneratorNum}", totalGeneratorNum.ToString());
                }
                else if (activatedGeneratorNum >= totalGeneratorNum)
                {
                    message = templateWhenFullyActivated;
                }

                return message;
            }

            public static string GetSpectatorsInformation()
            {
                //<color=#808080>Spectators:</color> {spectators_num}

                string template = "观察者数量：{spectators_num}";
                string message = String.Empty;

                if (Player.Get(RoleTypeId.Spectator).Count() < 10)
                {
                    message = template.Replace("{spectators_num}", "0" + Player.Get(RoleTypeId.Spectator).Count().ToString());
                }
                else
                {
                    message = template.Replace("{spectators_num}", Player.Get(RoleTypeId.Spectator).Count().ToString());
                }



                return message;
            }

            public static string GetNTFTicketInformation()
            {
                //<color=blue>NTF Spawn Chance:</color> {ntf_spawn_chance}%

                string template = "<color=#70C3FF>九尾狐部署票数：</color>{NTFTicketNum}";
                string message = String.Empty;

                if ((int)Respawn.NtfTickets < 10)
                {
                    message = template.Replace("{NTFTicketNum}", ("0" + (int)Respawn.NtfTickets).ToString());
                }
                else
                {
                    message = template.Replace("{NTFTicketNum}", ((int)Respawn.NtfTickets).ToString());
                }

                return message;
            }

            public static string GetCITicketInformation()
            {
                //<color=green>CI Spawn Chance:</color> {ci_spawn_chance}%

                string template = "<color=#008F1C>混沌分裂者部署票数：</color>{CITicketNum}";
                string message = String.Empty;

                if ((int)Respawn.ChaosTickets < 10)
                {
                    message = template.Replace("{CITicketNum}", ("0" + (int)Respawn.ChaosTickets).ToString());
                }
                else
                {
                    message = template.Replace("{CITicketNum}", ((int)Respawn.ChaosTickets).ToString());
                }


                return message;
            }

            private static int hintIndex = 0;
            private static TimeSpan lastTimeRefresh = TimeSpan.Zero;
            private static int timeToRefresh = Plugin.instance.Config.HintDisplayInterval;
            public static string GetHint()
            {
                string message = Plugin.instance.Config.Hints[hintIndex];

                var TimeInterval = Round.ElapsedTime.TotalSeconds - lastTimeRefresh.TotalSeconds;
                if (TimeInterval < timeToRefresh) return message;

                //If need refresh
                lastTimeRefresh = Round.ElapsedTime;

                if (hintIndex == Plugin.instance.Config.Hints.Count() - 1)
                {
                    hintIndex = 0;
                }
                else
                {
                    hintIndex++;
                }

                return message;
            }
        }
    }

    public class SCPTemplate : AliveTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.SCP;

        protected List<SCPEventHint> SCPInformations = new List<SCPEventHint>();

        //SCP Info Slots
        protected Hint[] infoSlots = new Hint[4]
        {
            new Hint(30, HintAlignment.Right, "", "infoSlot1", true).setFontSize(20),
            new Hint(50, HintAlignment.Right, "", "infoSlot2", true).setFontSize(20),
            new Hint(70, HintAlignment.Right, "", "infoSlot3", true).setFontSize(20),
            new Hint(90, HintAlignment.Right, "", "infoSlot4", true).setFontSize(20)
        };

        public SCPTemplate(Player player) : base(player)
        {
        }

        protected override void UpdateTopBar()
        {
            TopBar.message = $"TPS:{Server.Tps} | {player.Nickname}";
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            BottomBar.message = $"{player.DisplayNickname}|<color={player.Role.Color.ToHex()}>{Plugin.instance.Config.RoleName[player.Role.Type]}</color>|剩余{Player.List.Count(x => UICommonTools.IsSCP(x)) - 1}队友|<color=#7CB342>Meow</color>";
            BottomBar.hide = false;
        }

        public override void SetUpTemplate()
        {
            foreach (var slot in infoSlots)
            {
                playerDisplay.AddHint(slot);
            }

            BindEvents();

            base.SetUpTemplate();
        }

        public override void DestructTemplate()
        {
            foreach (var slot in infoSlots)
            {
                playerDisplay.RemoveHint(slot);
            }

            UnbindEvents();

            SCPInformations.Clear();

            base.DestructTemplate();
        }

        public override void UpdateTemplate()
        {
            CheckVisionInfo();

            SCPInformations.RemoveAll(x => Round.ElapsedTime - x.timeAdded > TimeSpan.FromSeconds(10));

            infoSlots[0].message = SCPInformations.Count > 0 ? SCPInformations[0]?.info : string.Empty;
            infoSlots[1].message = SCPInformations.Count > 1 ? SCPInformations[1]?.info : string.Empty;
            infoSlots[2].message = SCPInformations.Count > 2 ? SCPInformations[2]?.info : string.Empty;
            infoSlots[3].message = SCPInformations.Count > 3 ? SCPInformations[3]?.info : string.Empty;

            foreach (var slot in infoSlots)
            {
                slot.hide = string.IsNullOrEmpty(slot.message);
            }

            base.UpdateTemplate();
        }

        protected void BindEvents()
        {
            Exiled.Events.Handlers.Player.Hurt += OnHurting;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        }

        protected void UnbindEvents()
        {
            Exiled.Events.Handlers.Player.Hurt -= OnHurting;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        }

        protected Dictionary<Player, TimeSpan> lastTimeHurtInformed = new Dictionary<Player, TimeSpan>();
        protected void OnHurting(HurtEventArgs ev)
        {
            if (UICommonTools.IsSCP(ev.Player) && ev.Player.Role.Type != RoleTypeId.Scp0492 && ev.Player.Health <= ev.Player.MaxHealth * 0.2)
            {
                if (lastTimeHurtInformed.ContainsKey(ev.Player))
                {
                    if (Round.ElapsedTime - lastTimeHurtInformed[ev.Player] > TimeSpan.FromSeconds(40))
                    {
                        SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPLowHP, $"<b><color=#D32F2F>⚠</color></b>{Config.instance.RoleName[ev.Player.Role.Type]}的血量较低|血量：{(int)ev.Player.Health}", Round.ElapsedTime));
                        lastTimeHurtInformed[ev.Player] = Round.ElapsedTime;
                    }
                }
                else
                {
                    lastTimeHurtInformed.Add(ev.Player, Round.ElapsedTime);
                    SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPLowHP, $"<b><color=#D32F2F>⚠</color></b>{Config.instance.RoleName[ev.Player.Role.Type]}的血量较低|血量：{(int)ev.Player.Health}", Round.ElapsedTime));
                }
            }
        }

        protected void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (UICommonTools.IsSCP(ev.Player) && ev.Player.Role.Type != RoleTypeId.Scp0492 && ev.NewRole == RoleTypeId.Spectator)
            {
                SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPDeath, $"<b><color=#D32F2F>⚠</color></b>{Config.instance.RoleName[ev.Player.Role.Type]}已死亡", Round.ElapsedTime));
            }
        }

        protected Dictionary<Player, TimeSpan> lastTimeSpotted = new Dictionary<Player, TimeSpan>();
        protected void CheckVisionInfo()
        {
            if (Player.List.Count(x => x.IsHuman) <= 4)
            {
                foreach (Player player in Player.List)
                {
                    if (!UICommonTools.IsSCP(player))
                        continue;

                    foreach (Player target in Player.List)
                    {
                        if (!target.IsHuman || target.Role.Team == Team.OtherAlive)
                            continue;

                        if (!CanSee(player, target))
                            continue;

                        if (lastTimeSpotted.ContainsKey(player))
                        {
                            if (Round.ElapsedTime - lastTimeSpotted[player] > TimeSpan.FromSeconds(40))
                            {
                                SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SpotHuman, $"<b><color=#3385ff>⚪</color></b>{Config.instance.RoleName[player.Role.Type]}在{Config.instance.ZoneName[player.Zone]}找到了一个人类", Round.ElapsedTime));
                                lastTimeSpotted[player] = Round.ElapsedTime;
                            }
                        }
                        else
                        {
                            lastTimeSpotted.Add(player, Round.ElapsedTime);
                            SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPDeath, $"<b><color=#3385ff>⚪</color></b>{Config.instance.RoleName[player.Role.Type]}在{Config.instance.ZoneName[player.Zone]}找到了一个人类", Round.ElapsedTime));
                        }
                    }
                }
            }
            else
            {
                foreach (Player player in Player.List)
                {
                    if (!UICommonTools.IsSCP(player))
                        continue;

                    List<Player> targets = new List<Player>();

                    foreach (Player target in Player.List)
                    {
                        if (!target.IsHuman || target.Role.Team == Team.OtherAlive)
                            continue;

                        if (!CanSee(player, target))
                            continue;

                        if (lastTimeSpotted.ContainsKey(target))
                        {
                            if (Round.ElapsedTime - lastTimeSpotted[target] > TimeSpan.FromSeconds(40))
                            {
                                targets.Add(target);
                            }
                        }
                        else
                        {
                            targets.Add(target);
                        }
                    }

                    if (targets.Count >= 4)
                    {
                        SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SpotHuman2, $"⚪{Config.instance.RoleName[player.Role.Type]}在{player.Zone}找到了大量人类", Round.ElapsedTime));
                        foreach (Player target in targets)
                        {
                            if (lastTimeSpotted.ContainsKey(target))
                            {
                                lastTimeSpotted[target] = Round.ElapsedTime;
                            }
                            else
                            {
                                lastTimeSpotted.Add(target, Round.ElapsedTime);
                            }
                        }
                    }
                }
            }
        }

        protected bool CanSee(Player player, Player target)
        {
            if (!target.ReferenceHub.playerEffectsController.GetEffect<Invisible>().IsEnabled)
            {
                IFpcRole fpcRole = target.ReferenceHub.roleManager.CurrentRole as IFpcRole;
                if (fpcRole != null && VisionInformation.GetVisionInformation(player.ReferenceHub, player.CameraTransform, fpcRole.FpcModule.Position, fpcRole.FpcModule.CharacterControllerSettings.Radius, default, true, true, 0, false).IsLooking)
                {
                    return true;
                }
            }
            return false;
        }

        public class SCPEventHint
        {
            public enum SCPEventType
            {
                SCPDeath,
                SCPLowHP,
                SpotHuman,//sport human when there's <4 humans
                SpotHuman2//Spot >7 humans 
            }
            SCPEventType sCPEventType;
            public string info;
            public TimeSpan timeAdded;

            public SCPEventHint(SCPEventType type, string info, TimeSpan timeAdded)
            {
                this.sCPEventType = type;
                this.info = info;
                this.timeAdded = timeAdded;
            }
        }
    }

    public class CustomSCPTemplate : SCPTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.CustomSCP;

        public CustomSCPTemplate(Player player) : base(player)
        {
        }

        protected override void UpdateTopBar()
        {
            TopBar.message = $"TPS:{Server.Tps} | {player.Nickname}";
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            var playerRoleName = string.Empty;
            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.Check(player))
                {
                    playerRoleName = customRole.Name;
                    break;
                }
            }

            BottomBar.message = $"{player.DisplayNickname}|<color=#EC2222>{playerRoleName}</color>|剩余{Player.List.Count(x => UICommonTools.IsSCP(x)) - 1}队友|{UICommonTools.GetAmmoInfo(player)}|{UICommonTools.GetArmorInfo(player)}|<color=#7CB342>Meow</color>";
            BottomBar.hide = false;
        }
    }

    public class CustomHumanTemplate : GeneralHumanTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.CustomHuman;

        public CustomHumanTemplate(Player player) : base(player)
        {
        }

        protected override void UpdateTopBar()
        {
            TopBar.message = $"TPS:{Server.Tps} | {player.Nickname}";
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            var playerRoleName = string.Empty;
            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.Check(player))
                {
                    playerRoleName = customRole.Name;
                    break;
                }
            }

            BottomBar.message = $"{player.DisplayNickname}|<color={player.Role.Color.ToHex()}>{playerRoleName}</color>|{UICommonTools.GetAmmoInfo(player)}|{UICommonTools.GetArmorInfo(player)}|<color=#7CB342>Meow</color>";
            BottomBar.hide = false;
        }
    }

    public static class UICommonTools
    {
        /// <summary>
        /// This code return a value that indicate whether a player is a scp, this include custom roles that include "SCP" in their name
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsSCP(Player player)
        {
            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.Check(player) && customRole.Name.Contains("SCP"))
                {
                    return true;
                }
            }

            return player.Role.Team == Team.SCPs;
        }

        public static bool IsCustomRole(Player player)
        {
            if(CustomRole.Registered.Any(x => x.Check(player)))
            {
                return true;
            }

            return false;
        }

        public static CustomRole GetCustomRole(Player player)
        {
            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.Check(player))
                {
                    return customRole;
                }
            }

            return null;
        }

        public static string GetArmorInfo(Player player)
        {
            if (player.CurrentArmor != null)
            {
                return Plugin.instance.Config.ItemName[player.CurrentArmor.Type];
            }

            return Plugin.instance.Config.NoArmor;
        }

        public static string GetAmmoInfo(Player player)
        {
            Dictionary<ItemType, ushort> ammos = 
                player.Ammo
                .Where(x => x.Key != ItemType.None && x.Value > 0)
                .ToDictionary(x => x.Key, x => x.Value);

            string ammoStatus;

            if (player.CurrentItem is Firearm firearm)
            {
                AmmoType ammoType = firearm.AmmoType;
                ItemType itemType = ammoType.GetItemType();

                if (!ammos.ContainsKey(itemType) || ammos[itemType] == 0)
                {
                    ammoStatus = Plugin.instance.Config.NoAmmo;
                }
                else
                {
                    ammoStatus = Plugin.instance.Config.AmmoHint
                    .Replace("{Ammo}", Plugin.instance.Config.AmmoName[ammoType])
                    .Replace("{NumOfAmmo}", ammos[itemType].ToString());
                }
            }
            else
            {
                var numOfAmmoTypes = ammos.Keys.Count;

                if (numOfAmmoTypes <= 0)
                {
                    ammoStatus = Plugin.instance.Config.NoAmmo;
                }
                else if (numOfAmmoTypes <= 1)
                {
                    ammoStatus = Plugin.instance.Config.AmmoHint
                        .Replace("{Ammo}", Plugin.instance.Config.ItemName[ammos.First().Key])
                        .Replace("{NumOfAmmo}", ammos.First().Value.ToString());
                }
                else
                {
                    var totalAmmos = 0;

                    foreach (var numOfAmmo in ammos.Values)
                    {
                        totalAmmos += numOfAmmo;
                    }

                    ammoStatus = Plugin.instance.Config.AmmoHint2
                        .Replace("{NumOfAmmo}", totalAmmos.ToString());
                }
            }

            return ammoStatus;
        }
    }
}
