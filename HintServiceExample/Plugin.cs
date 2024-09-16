﻿using System;
using System.Text;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.UI.Extension;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

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
            //ShowHintA(ev.Player);
            //ShowHintB(ev.Player);
            //ShowDynamicHintA(ev.Player);
            //ShowCommonHintA(ev.Player);

            //ev.Player.ShowHint("<Line-Height=500>\n<pos=400>Hello, this is a hint using ShowHint directly", 20f); //Compatibility adapter

            ev.Player.GetPlayerDisplay().AddHint(new Hint
            {
                AutoText = param => Hotbar(Player.Get(param.Player)),
                FontSize = 17,
                YCoordinate = 980
            });
        }

        private static string Hotbar(Player player)
        {
            var playtime = Round.ElapsedTime;
            var steamId = player.UserId;

            int days = playtime.Days;
            int hours = playtime.Hours;
            int minutes = playtime.Minutes;
            int seconds = playtime.Seconds;

            int playerLevel = 1;
            int playerXP = 40;
            int requiredXP = 150;

            var builder = new StringBuilder();
            builder.Clear();

            string rankName = "ServerOwner";

            builder.Append($"Spielername: <color=#00bfcf>{player.Nickname}</color> | Rang: <color=#00bfcf>{rankName}</color> | Level: <color=#00bfcf>{playerLevel}</color> | XP: <color=#00bfcf>{playerXP}/{requiredXP}</color> | Prestige: <color=#00bfcf>0</color> | Playtime: <color=#00bfcf>{days}d {hours}h {minutes}m {seconds}s</color>\n\n");
            builder.Append($"[ UserID: <color=#00bfcf>{steamId}</color> ]");

            string hintText = builder.ToString();

            return hintText;
        }

        //How to use Hint
        private static void ShowHintA(Player player)
        {
            var pd = player.GetPlayerDisplay();

            var nameHint = new Hint()
            {
                Text = $"<size=100>Hello</size>, {player.Nickname}",
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
                    Text = "Welcome\n<b>To HintServiceMeow</b>",
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
