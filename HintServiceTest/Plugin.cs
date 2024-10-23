using System;
using System.Collections.Generic;
using System.Linq;
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
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace HintServiceTest
{
    /// <summary>
    /// This is an Exiled only example of how to create a simple ui for players using Hint and PlayerDisplay.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        public override string Name => "HintServiceTest";

        public override void OnEnabled()
        {
            Timing.RunCoroutine(TestB.Hud());

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
        }
    }

    public static class TestB
    {
        public static IEnumerator<float> Hud()
        {
            while (true)
            {
                try
                {
                    foreach (Player player in Player.List)
                    {
                        //Get the hint from the PlayerDisplay
                        var hint = player.GetPlayerDisplay().GetHint(player.Nickname + "HudHint");

                        //If hint not added, then add it.
                        if (hint == null)
                        {
                            hint = new Hint
                            {
                                Id = player.Nickname + "HudHint",
                                Alignment = HintAlignment.Left,
                                YCoordinate = 1080,
                                YCoordinateAlign = HintVerticalAlign.Bottom,
                                FontSize = 20,
                                XCoordinate = 70,
                                Hide = true
                            };
                            player.GetPlayerDisplay().AddHint(hint);
                        }

                        if(!player.IsAlive || player.IsOverwatchEnabled || player.Role.Type == RoleTypeId.Spectator || player.Role.Type == RoleTypeId.Overwatch)
                            continue;

                        //Set hint's text
                        string color = player.Role.Color.ToHex();
                        string roleColor = ColorUtility.ToHtmlStringRGB(player.Role.Color);
                        string roleInfo = player.CustomInfo ?? GetCustomRoleName(player.Role.Type, player.Role.Color);
                        string roundTime = $"{Math.Floor(Round.ElapsedTime.TotalMinutes)}:{Round.ElapsedTime.Seconds:D2}";
                        hint.Text = $"Ваш ник: {player.Nickname} \nВаша роль: <color=#{roleColor}> {roleInfo}</color> \nРаунд идёт: {roundTime}</voffset></align></size>";
                        hint.Hide = false;

                        hint.HideAfter(1.5f);
                    }
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private static string GetCustomRoleName(RoleTypeId role, Color roleColor)
        {
            string roleName = role.ToString();
            string colorString = ColorUtility.ToHtmlStringRGB(roleColor);
            return $"<color=#{colorString}>{roleName}</color>";
        }
    }
}
