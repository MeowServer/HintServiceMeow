using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

namespace HintServiceMeow.UITemplates
{
    internal class SpectatorTemplate : PlayerUITemplateBase
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
                    .Replace("{RespawnTeam}", Config.PluginConfig.instance.GeneralConfig.RespawnTeamDictionary[Respawn.NextKnownTeam]);//Team
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
                    .Replace("{WarheadStatus}", Config.PluginConfig.instance.GeneralConfig.WarheadStatusDictionary[Warhead.Status])
                    .Replace("{TimeRemaining}", "<color=#d0652f>" + ((int)Warhead.DetonationTimer).ToString() + "秒" + "</color>");
                }
                else
                {
                    message = template
                    .Replace("{WarheadStatus}", Config.PluginConfig.instance.GeneralConfig.WarheadStatusDictionary[Warhead.Status])
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
            private static TimeSpan timeToRefresh = TimeSpan.FromSeconds(Config.PluginConfig.instance.SpectatorTemplateConfig.HintDisplayInterval);
            public static string GetHint()
            {
                var TimeInterval = Round.ElapsedTime - lastTimeRefresh;
                if (TimeInterval > timeToRefresh)
                {
                    //If need refresh
                    lastTimeRefresh = Round.ElapsedTime;

                    if (hintIndex == Config.PluginConfig.instance.SpectatorTemplateConfig.Hints.Count() - 1)
                    {
                        hintIndex = 0;
                    }
                    else
                    {
                        hintIndex++;
                    }
                }

                return Config.PluginConfig.instance.SpectatorTemplateConfig.Hints[hintIndex];
            }
        }
    }
}
