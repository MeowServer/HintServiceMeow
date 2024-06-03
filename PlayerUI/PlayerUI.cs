using Exiled.API.Features;
using HintServiceMeow.UITemplates;
using MEC;
using PlayerRoles;

using System;
using System.Collections.Generic;
using System.Linq;

namespace HintServiceMeow
{
    /// <summary>
    /// A UI based on PlayerDisplay
    /// contain 3 main parts: common hints, UI Template, and player effects
    /// 
    /// player effects had not been implemented yet
    /// </summary>
    public class PlayerUI
    {
        private static List<PlayerUI> playerUIs = new List<PlayerUI>();

        private Player player;
        private PlayerDisplay playerDisplay;

        //Effects
        private CoroutineHandle effectsUpdateCoroutine;

        private List<UIEffectBase> effects = new List<UIEffectBase>();

        //Common Hints
        private CoroutineHandle commonHintUpdateCoroutine;

        private TimeSpan itemHintTimeToRemove = TimeSpan.MinValue;
        private DynamicHint[] itemHints = new DynamicHint[2]
        {
            new DynamicHint(450, 700, HintAlignment.Center, "", "itemHint1", true).setFontSize(25),
            new DynamicHint(450+25, 700+25, HintAlignment.Center, "", "itemHint2", true).setFontSize(25)
        };

        private TimeSpan mapHintTimeToRemove = TimeSpan.MinValue;
        private DynamicHint[] mapHints = new DynamicHint[2]
        {
            new DynamicHint(0, 200, HintAlignment.Right, "", "mapHint1", true).setFontSize(25),
            new DynamicHint(0+25, 200+25, HintAlignment.Right, "", "mapHint2", true).setFontSize(25)
        };

        private TimeSpan roleHintTimeToRemove = TimeSpan.MinValue;
        private DynamicHint[] roleHints = new DynamicHint[4]
        {
            new DynamicHint(100, 600, HintAlignment.Left, "", "roleTitle", true).setFontSize(30),
            new DynamicHint(100+30, 600+30 * 1, HintAlignment.Left, "", "roleDescription1", true).setFontSize(25),
            new DynamicHint(100+50, 600+50, HintAlignment.Left, "", "roleDescription1", true).setFontSize(25),
            new DynamicHint(100+70, 600+70, HintAlignment.Left, "", "roleDescription1", true).setFontSize(25)
        };

        private TimeSpan otherHintTimeToRemove = TimeSpan.MinValue;
        private DynamicHint[] otherHints = new DynamicHint[4]
        {
            new DynamicHint(500, 700, HintAlignment.Center, "", "otherHint1", true),
            new DynamicHint(520, 700, HintAlignment.Center, "", "otherHint2", true),
            new DynamicHint(540, 700, HintAlignment.Center, "", "otherHint3", true),
            new DynamicHint(560, 700, HintAlignment.Center, "", "otherHint4", true),
        };

        //Template
        private PlayerUITemplateBase template;
        private CoroutineHandle templateUpdateCoroutine;

        //Private Effect Methods
        private IEnumerator<float> effectCoroutineMethod()
        {
            while (true)
            {
                foreach(UIEffectBase effect in effects)
                {
                    effect.UpdateEffect();
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private void SetUpEffect()
        {
            effectsUpdateCoroutine = Timing.RunCoroutine(effectCoroutineMethod());
        }

        private void DestructEffect()
        {
            if(effectsUpdateCoroutine.IsRunning)
            {
                Timing.KillCoroutines(effectsUpdateCoroutine);
            }

            foreach(UIEffectBase effect in effects)
            {
                effect.DestructEffect();
            }
        }

        //Public Effect Methods
        public void AddEffect(UIEffectBase effect)
        {
            effect.SetEffect();
            effects.Add(effect);
        }

        public void AddEffect<EffectType>()
        {
            EffectType instance = (EffectType)Activator.CreateInstance(typeof(EffectType));

            if(instance is UIEffectBase effect)
            {
                AddEffect(effect);
            }
        }

        public void RemoveEffect(UIEffectBase effect)
        {
            effect.DestructEffect();
            effects.Remove(effect);
        }

        public void RemoveEffect<EffectType>()
        {
            foreach(UIEffectBase effect in effects)
            {
                if(effect is EffectType)
                {
                    RemoveEffect(effect);
                    return;
                }
            }
        }

        //Private Common Hints Methods
        private IEnumerator<float> CommonHintCoroutineMethod()
        {
            while (true)
            {
                try
                {
                    TimeSpan currentTime = Round.ElapsedTime;

                    if (currentTime > itemHintTimeToRemove)
                    {
                        itemHints[0].hide = true;
                        itemHints[1].hide = true;
                    }

                    if (currentTime > mapHintTimeToRemove)
                    {
                        mapHints[0].hide = true;
                        mapHints[1].hide = true;
                    }

                    if (currentTime > roleHintTimeToRemove)
                    {
                        foreach (DynamicHint hint in roleHints)
                        {
                            hint.hide = true;
                        }
                    }

                    if (currentTime > otherHintTimeToRemove)
                    {
                        foreach (DynamicHint hint in otherHints)
                        {
                            hint.hide = true;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private void SetUpCommonHints()
        {
            playerDisplay.AddHints(itemHints);
            playerDisplay.AddHints(mapHints);
            playerDisplay.AddHints(roleHints);
            playerDisplay.AddHints(otherHints);

            commonHintUpdateCoroutine = Timing.RunCoroutine(CommonHintCoroutineMethod());
        }

        private void DestructCommonHints()
        {
            if(commonHintUpdateCoroutine.IsRunning)
            {
                Timing.KillCoroutines(commonHintUpdateCoroutine);
            }

            playerDisplay.RemoveHints(itemHints);
            playerDisplay.RemoveHints(mapHints);
            playerDisplay.RemoveHints(roleHints);
            playerDisplay.RemoveHints(otherHints);
        }

        //Public Common Item Hints Methods
        public void ShowItemHint(string itemName)
        {
            ShowItemHint(itemName, 4);
        }

        public void ShowItemHint(string itemName, int time)
        {
            itemHintTimeToRemove = Round.ElapsedTime + TimeSpan.FromSeconds(time);

            itemHints[0].message = itemName;
            itemHints[0].hide = false;
        }

        public void ShowItemHint(string itemName, string description)
        {
            ShowItemHint(itemName, description, 7);
        }

        public void ShowItemHint(string itemName, string description, int time)
        {
            itemHintTimeToRemove = Round.ElapsedTime + TimeSpan.FromSeconds(time);

            itemHints[0].message = itemName;
            itemHints[0].hide = false;

            itemHints[1].message = description;
            itemHints[1].hide = false;
        }

        //Public Common Map Hints Methods
        public void ShowMapHint(string roomName)
        {
            ShowMapHint(roomName, 4);
        }

        public void ShowMapHint(string roomName, int time)
        {
            mapHintTimeToRemove = Round.ElapsedTime + TimeSpan.FromSeconds(time);

            mapHints[0].message = roomName;
            mapHints[0].hide = false;
        }

        public void ShowMapHint(string roomName, string description)
        {
            ShowMapHint(roomName, description, 7);
        }

        public void ShowMapHint(string roomName, string description, int time)
        {
            mapHintTimeToRemove = Round.ElapsedTime + TimeSpan.FromSeconds(time);

            mapHints[0].message = roomName;
            mapHints[0].hide = false;

            mapHints[1].message = description;
            mapHints[1].hide = false;
        }

        //Public Common Role Hints Methods
        public void ShowRoleHint(string roleName)
        {
            ShowRoleHint(roleName, 4);
        }

        public void ShowRoleHint(string roleName, int time)
        {
            roleHintTimeToRemove = Round.ElapsedTime + TimeSpan.FromSeconds(time);

            roleHints[0].message = roleName;
            roleHints[0].hide = false;
        }

        public void ShowRoleHint(string roleName, string[] description)
        {
            ShowRoleHint(roleName, description, 7);
        }

        public void ShowRoleHint(string roleName, string[] description, int time)
        {
            roleHintTimeToRemove = Round.ElapsedTime + TimeSpan.FromSeconds(time);

            roleHints[0].message = roleName;
            roleHints[0].hide = false;

            for(int i = 0; i < description.Length; i++)
            {
                if(i >= roleHints.Length - 1) 
                    break;

                roleHints[i + 1].message = $"• {description[i]}";
                roleHints[i + 1].hide = false;
            }
        }

        //Public Common Other Hints Methods
        public void ShowOtherHint(string hint)
        {
            ShowOtherHint(hint, 4);
        }

        public void ShowOtherHint(string hint, int time)
        {
            ShowOtherHint(new string[]
            {
                hint,
            }, time);
        }

        public void ShowOtherHint(string[] hints)
        {
            ShowOtherHint(hints, 7);
        }

        public void ShowOtherHint(string[] hints, int time)
        {
            otherHintTimeToRemove = Round.ElapsedTime + TimeSpan.FromSeconds(time);

            for(int i = 0; i < otherHints.Length; i++)
            {
                otherHints[i].message = hints.Length > i ? hints[i] : string.Empty;
                otherHints[i].hide = otherHints[i].message == string.Empty;
            }
        }

        //Private Template Methods
        private void SetTemplate()
        {
            StopTemplateCoroutine();

            template?.DestructTemplate();

            if(UICommonTools.IsCustomRole(player))
            {
                if(UICommonTools.IsSCP(player))
                {
                    template = new CustomSCPTemplate(player);
                }
                else
                {
                    template = new CustomHumanTemplate(player);
                }
            }
            else
            {
                if (player.IsAlive && player.IsHuman)
                {
                    template = new GeneralHumanTemplate(player);
                }
                else if (player.IsAlive && player.IsScp)
                {
                    template = new SCPTemplate(player);
                }
                else if (player.Role.Type == RoleTypeId.Spectator)
                {
                    template = new SpectatorTemplate(player);
                }
                else
                {
                    template = null;
                }
            }

            StartTemplateCoroutine();
        }

        private bool CheckTemplate()
        {
            bool isCorrectType;

            if (UICommonTools.IsCustomRole(player))
            {
                if (UICommonTools.IsSCP(player))
                {
                    isCorrectType = template?.type == PlayerUITemplateBase.PlayerUITemplateType.CustomSCP;
                }
                else
                {
                    isCorrectType = template?.type == PlayerUITemplateBase.PlayerUITemplateType.CustomHuman;
                }
            }
            else
            {
                if (player.IsAlive && player.IsHuman)
                {
                    isCorrectType = template?.type == PlayerUITemplateBase.PlayerUITemplateType.GeneralHuman;
                }
                else if (player.IsAlive && player.IsScp)
                {
                    isCorrectType = template?.type == PlayerUITemplateBase.PlayerUITemplateType.SCP;
                }
                else if (player.Role.Type == RoleTypeId.Spectator)
                {
                    isCorrectType = template?.type == PlayerUITemplateBase.PlayerUITemplateType.Spectator;
                }
                else
                {
                    isCorrectType = template == null;
                }
            }

            return isCorrectType;
        }

        private IEnumerator<float> TemplateUpdateCoroutineMethod()
        {
            while (true)
            {
                if (!player.IsConnected)
                {
                    yield return Timing.WaitForSeconds(0.1f);
                    continue;
                }

                try
                {
                    if (!CheckTemplate())
                    {
                        SetTemplate();
                    }

                    template?.UpdateTemplate();
                }
                catch (Exception ex)
                {
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private void StartTemplateCoroutine()
        {
            templateUpdateCoroutine = Timing.RunCoroutine(TemplateUpdateCoroutineMethod());
        }

        private void StopTemplateCoroutine()
        {
            Timing.KillCoroutines(templateUpdateCoroutine);
        }

        //Private PlayerUI methods
        internal PlayerUI(Player player)
        {
            if(playerUIs.Any(x => x.player == player))
            {
                Log.Error($"A PlayerUI for this player had already been created for this player : {player.Nickname}");
                return;
            }

            this.player = player;
            this.playerDisplay = PlayerDisplay.Get(player);
            //effect
            SetUpEffect();
            //common hints
            SetUpCommonHints();
            //template
            SetTemplate();
            StartTemplateCoroutine();

            playerUIs.Add(this);
        }

        internal static void RemovePlayerUI(Player player)
        {
            playerUIs.Find(x => x.player == player)?.Destruct();
        }

        internal void Destruct()
        {
            //Effects
            DestructEffect();

            //Common Hints
            DestructCommonHints();

            //Template
            StopTemplateCoroutine();
            template?.DestructTemplate();

            playerUIs.Remove(this);
        }

        internal static void ClearAllPlayerUI()
        {
            foreach (PlayerUI ui in playerUIs)
            {
                ui.Destruct();
            }
        }

        //Public PlayerUI tools
        public static PlayerUI Get(Player player)
        {
            return playerUIs.Find(x => x.player == player);
        }

        public static PlayerUI Get(PlayerDisplay display)
        {
            return playerUIs.Find(x => x.playerDisplay == display);
        }
    }

}
