using CustomPlayerEffects;
using Exiled.Events.EventArgs.Player;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using HintServiceMeow.Config;

namespace HintServiceMeow.UITemplates
{
    internal abstract class AliveTemplate : PlayerUITemplateBase
    {
        //Top Bar
        public Hint TopBar = new Hint(0, HintAlignment.Center, "", "TopBar1", true).setFontSize(15);

        //Bottom Bar
        public Hint BottomBar = new Hint(710, HintAlignment.Center, "", "BottomBar1", true).setFontSize(20);

        private DynamicHint spectatorTip = new DynamicHint(100, 300, HintAlignment.Right, "", "SpectatorHints1", true).setFontSize(20);
        private DynamicHint[] spectatorHints = new DynamicHint[4]
        {
            new DynamicHint(120, 320, HintAlignment.Right, "", "SpectatorHints1", true).setFontSize(20),
            new DynamicHint(140, 340, HintAlignment.Right, "", "SpectatorHints2", true).setFontSize(20),
            new DynamicHint(160, 360, HintAlignment.Right, "", "SpectatorHints3", true).setFontSize(20),
            new DynamicHint(180, 380, HintAlignment.Right, "", "SpectatorHints4", true).setFontSize(20)
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
            var spectatingPlayers = PlayerUICommonTools.GetSpectatorInfo(player);

            spectatorTip.hide = true;
            spectatorHints.ForEach(x => x.hide = true);

            if(spectatingPlayers.Count > 0)
            {
                spectatorTip.message = "👥观察者";
                spectatorTip.hide = false;
            }

            for (int i = 0; i < spectatingPlayers.Count && i < spectatorHints.Count(); i++)
            {
                spectatorHints[i].message = $"-{spectatingPlayers[i].Nickname}";
                spectatorHints[i].hide = false;
            }

            if(spectatingPlayers.Count > spectatorHints.Count())
            {
                spectatorHints[spectatorHints.Count() - 1].message = $"共{spectatingPlayers.Count}个观察者正在观察您";
            }

        }
    }
    internal class GeneralHumanTemplate : AliveTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.GeneralHuman;

        public GeneralHumanTemplate(Player player) : base(player)
        {

        }

        protected override void UpdateTopBar()
        {
            string template = PluginConfig.instance.GeneralHumanTemplateConfig.TopBar;

            TopBar.message = PlayerUICommonTools.GetContent(template, player);
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            string template = PluginConfig.instance.GeneralHumanTemplateConfig.BottomBar;

            BottomBar.message = PlayerUICommonTools.GetContent(template, player);
            BottomBar.hide = false;
        }
    }

    internal class SCPTemplate : AliveTemplate
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
            string template = PluginConfig.instance.ScpTemplateConfig.TopBar;

            TopBar.message = PlayerUICommonTools.GetContent(template, player);
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            string template = PluginConfig.instance.ScpTemplateConfig.BottomBar;

            BottomBar.message = PlayerUICommonTools.GetContent(template, player);
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
            try
            {
                if (PlayerUICommonTools.IsSCP(ev.Player) && ev.Player.Role.Type != RoleTypeId.Scp0492 && ev.Player.Health <= ev.Player.MaxHealth * 0.2)
                {
                    if (lastTimeHurtInformed.ContainsKey(ev.Player))
                    {
                        if (Round.ElapsedTime - lastTimeHurtInformed[ev.Player] > TimeSpan.FromSeconds(40))
                        {
                            SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPLowHP, $"<b><color=#D32F2F>⚠</color></b>{PluginConfig.instance.GeneralConfig.RoleName[ev.Player.Role.Type]}的血量较低|血量：{(int)ev.Player.Health}", Round.ElapsedTime));
                            lastTimeHurtInformed[ev.Player] = Round.ElapsedTime;
                        }
                    }
                    else
                    {
                        lastTimeHurtInformed.Add(ev.Player, Round.ElapsedTime);
                        SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPLowHP, $"<b><color=#D32F2F>⚠</color></b>{PluginConfig.instance.GeneralConfig.RoleName[ev.Player.Role.Type]}的血量较低|血量：{(int)ev.Player.Health}", Round.ElapsedTime));
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        protected void OnChangingRole(ChangingRoleEventArgs ev)
        {
            try
            {
                if (PlayerUICommonTools.IsSCP(ev.Player) && ev.Player.Role.Type != RoleTypeId.Scp0492 && ev.NewRole == RoleTypeId.Spectator)
                {
                    SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPDeath, $"<b><color=#D32F2F>⚠</color></b>{PluginConfig.instance.GeneralConfig.RoleName[ev.Player.Role.Type]}已死亡", Round.ElapsedTime));
                }
            }catch(Exception ex)
            {

            }
        }

        protected Dictionary<Player, TimeSpan> lastTimeSpotted = new Dictionary<Player, TimeSpan>();
        protected void CheckVisionInfo()
        {
            try
            {
                if (Player.List.Count(x => x.IsHuman) <= 4)
                {
                    foreach (Player player in Player.List)
                    {
                        if (!PlayerUICommonTools.IsSCP(player))
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
                                    SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SpotHuman, $"<b><color=#3385ff>⚪</color></b>{PluginConfig.instance.GeneralConfig.RoleName[player.Role.Type]}在{PluginConfig.instance.GeneralConfig.ZoneName[player.Zone]}找到了一个人类", Round.ElapsedTime));
                                    lastTimeSpotted[player] = Round.ElapsedTime;
                                }
                            }
                            else
                            {
                                lastTimeSpotted.Add(player, Round.ElapsedTime);
                                SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SCPDeath, $"<b><color=#3385ff>⚪</color></b>{PluginConfig.instance.GeneralConfig.RoleName[player.Role.Type]}在{PluginConfig.instance.GeneralConfig.ZoneName[player.Zone]}找到了一个人类", Round.ElapsedTime));
                            }
                        }
                    }
                }
                else
                {
                    foreach (Player player in Player.List)
                    {
                        if (!PlayerUICommonTools.IsSCP(player))
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
                            SCPInformations.Insert(0, new SCPEventHint(SCPEventHint.SCPEventType.SpotHuman2, $"⚪{PluginConfig.instance.GeneralConfig.RoleName[player.Role.Type]}在{player.Zone}找到了大量人类", Round.ElapsedTime));
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
            catch (Exception e)
            {
                
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
}
