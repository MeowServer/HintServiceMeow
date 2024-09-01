using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.UI.Extension;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;
using MEC;
using System.Collections.Generic;
using HintServiceMeow.UI.Utilities;

namespace HintServiceExample
{
    /// <summary>
    /// This is an Exiled only example of how to create a simple ui for players using Hint and PlayerDisplay.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        public override string Name => "HintServiceExample";

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.Verified += EventHandler.OnVerified;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= EventHandler.OnVerified;

            base.OnDisabled();
        }
    }

    public static class EventHandler
    {
        public static void OnVerified(VerifiedEventArgs ev)
        {
            ShowHintA(ev.Player);
            ShowHintB(ev.Player);
            ShowDynamicHintA(ev.Player);
            ShowCommonHintA(ev.Player);

            var ui = ev.Player.GetPlayerUi();
            ui.Style.SetStyle(-50, 1080, Style.StyleType.Italic);
            ui.Style.SetColor(-50, 1080, UnityEngine.Color.green);

        }

        //How to use Hint
        private static void ShowHintA(Player player)
        {
            var pd = player.GetPlayerDisplay();

            var nameHint = new Hint()
            {
                Text = $"Hello, {player.Nickname}",
                YCoordinateAlign = HintVerticalAlign.Top,
                YCoordinate = 0,
                Alignment = HintAlignment.Left,
                FontSize = 20
            };

            var currentTimeHint = new Hint()
            {
                AutoText = GetCurrentTime, //Use delegate to get text instead of static text
                YCoordinateAlign = HintVerticalAlign.Top,
                YCoordinate = 0,
                Alignment = HintAlignment.Right,
                FontSize = 20
            };

            var tPSHint = new Hint()
            {
                AutoText = GetTPS,
                YCoordinateAlign = HintVerticalAlign.Top,
                YCoordinate = 0,
                XCoordinate = 0,
                FontSize = 20
            };

            pd.AddHint(nameHint);
            pd.AddHint(currentTimeHint);
            pd.AddHint(tPSHint);
        }

        //Advanced Hint skills
        private static void ShowHintB(Player player)
        {
            var pd = player.GetPlayerDisplay();

            var roleHint = new Hint()
            {
                AutoText = GetRole,
                YCoordinateAlign = HintVerticalAlign.Bottom,
                YCoordinate = 1080, //1920*1080, so 1080 is the highest Y coordinate
                XCoordinate = 600,
                FontSize = 20
            };

            var itemHint = new Hint()
            {
                AutoText = GetItem,
                YCoordinateAlign = HintVerticalAlign.Bottom,
                YCoordinate = 1080,
                XCoordinate = -600,
                FontSize = 20,
                SyncSpeed = HintSyncSpeed.Fast // This will update the hint as soon as it can
            };

            pd.AddHint(roleHint);
            pd.AddHint(itemHint);
        }

        //How to use Dynamic Hint
        private static void ShowDynamicHintA(Player player)
        {
            for(var i = 0; i < 10; i++)
            {
                //Each dynamic hint will automatically find the position that does not overlaps with other hints
                player.GetPlayerDisplay().AddHint(new DynamicHint()
                {
                    Id = $"DynamicHint{i}",
                    Text = "Welcome \nTo HintServiceMeow",
                    TargetX = 0,
                    TargetY = 700,
                });
            }
        }

        //How to use Common Hint
        private static void ShowCommonHintA(Player player)
        {
            var ui = player.GetPlayerUi();

            ui.CommonHint.ShowRoleHint(player.Role.Name, "This is a decription of your role", 20f);
        }

        private static string GetCurrentTime(AbstractHint.TextUpdateArg ev)
        {
            return $"Current Time: {DateTime.Now:T}";
        }

        private static string GetTPS(AbstractHint.TextUpdateArg ev)
        {
            return $"TPS: {Server.Tps:0.#}";
        }

        private static string GetRole(AbstractHint.TextUpdateArg ev)
        {
            return Player.Get(ev.Player).Role?.Type.ToString();
        }

        private static string GetItem(AbstractHint.TextUpdateArg ev)
        {
            return Player.Get(ev.Player).CurrentItem?.Type.ToString()??ItemType.None.ToString();
        }
    }
}
