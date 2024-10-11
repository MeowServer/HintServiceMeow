using System;
using System.Collections.Generic;
using Exiled.API.Features;

using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Extension;
using HintServiceMeow.UI.Utilities;

using MEC;
using PlayerRoles;

using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace HintServiceTest
{
    /// <summary>
    /// This is an Exiled only example of how to create a simple ui for players using Hint and PlayerDisplay.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        public override string Name => "HintServiceExample";

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Verified += EventHandler.OnVerified;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.Verified -= EventHandler.OnVerified;

            base.OnDisabled();
        }

        public static void OnVerified(VerifiedEventArgs ev)
        {

        }
    }

    public static class EventHandler
    {
        public static void OnVerified(VerifiedEventArgs ev)
        {
            //Reported 
            Timing.RunCoroutine(Hud());
        }

        public static IEnumerator<float> Hud()
        {
            while (true)
            {
                foreach (Player player in Player.List)
                {
                    if (player.Role != RoleTypeId.Spectator)
                    {
                        string color = player.Role.Color.ToHex();

                        ShowMeowHint(player, 2f, $" <b><color=#F0FFF0>:credit_card: Имя:</color> <color={color}>{player.DisplayNickname}</b></color>", HintVerticalAlign.Middle, 1025, 55, HintAlignment.Left, 20);
                    }
                    else
                    {
                        var role = (SpectatorRole)player.Role; ;
                        Player spectator = player;
                        Player target = role.SpectatedPlayer;

                        if (target != null)
                        {
                            string color = target.Role.Color.ToHex();

                            ShowMeowHint(spectator, 2f, $"<b><color=#F0FFF0>:tv: Наблюдаемый игрок:</color> <color={color}>{target.DisplayNickname}</b></color>", HintVerticalAlign.Middle, 1025, 55, HintAlignment.Left, 20);
                        }
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        public static void ShowMeowHint(this Player ply, float time, string text, HintVerticalAlign Verticalalign = HintVerticalAlign.Top, int ycoordinate = 725, int xcoordinate = 0, HintAlignment aligmenthint = HintAlignment.Center, int fontsize = 27, int degradation = 30)
        {
            if (text.Contains("\n"))
            {
                string[] lines = text.Split('\n');
                int totalYCoordinate = ycoordinate;

                foreach (string line in lines)
                {
                    ShowMeowHint(ply, time, line, Verticalalign, totalYCoordinate, xcoordinate, aligmenthint, fontsize);
                    totalYCoordinate += degradation;
                }
            }
            else
            {
                text += "</s></color></b></u></i>";
                var hint = new Hint()
                {
                    Text = text,
                    YCoordinateAlign = Verticalalign,
                    YCoordinate = ycoordinate,
                    XCoordinate = xcoordinate,
                    Alignment = aligmenthint,
                    FontSize = fontsize
                };
                var playerDisplay = PlayerDisplay.Get(ply);
                playerDisplay.AddHint(hint);
                Timing.CallDelayed(time, () =>
                {
                    playerDisplay.RemoveHint(hint);
                    playerDisplay.ForceUpdate();
                });
            }
        }
    }
}
