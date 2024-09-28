using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.UI.Extension;
using MEC;
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
            //Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Verified += EventHandler.OnVerified;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            //Exiled.Events.Handlers.Player.Verified -= OnVerified;
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
            ShowHintA(ev.Player);
            ShowHintB(ev.Player);
            ShowDynamicHintA(ev.Player);
            ShowCommonHintA(ev.Player);

            Timing.RunCoroutine(CoroutineMethod(ev.Player));

            ev.Player.ShowHint("<Line-Height=500>\n<pos=400>Hello, this is a hint using ShowHint directly", 20f); //Compatibility adapter
        }

        //Basic Hint Skill
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
                AutoText = ev => Player.Get(ev.Player).Role?.Type.ToString(),
                YCoordinateAlign = HintVerticalAlign.Bottom,
                YCoordinate = 1080, //0 - 1080. This applies to any screen resolution since 1080 is virtual resolution
                XCoordinate = 600,
                FontSize = 20
            };

            var itemHint = new Hint()
            {
                AutoText = ev => Player.Get(ev.Player).CurrentItem?.Type.ToString() ?? ItemType.None.ToString(),
                YCoordinateAlign = HintVerticalAlign.Bottom,
                YCoordinate = 1080,
                XCoordinate = -600,
                FontSize = 20,
                SyncSpeed = HintSyncSpeed.Fastest // This will update the hint as soon as it can
            };

            pd.AddHint(roleHint);
            pd.AddHint(itemHint);
        }

        //How to use Dynamic Hint
        private static void ShowDynamicHintA(Player player)
        {
            for (var i = 0; i < 10; i++)
            {
                //Each dynamic hint will automatically find the position that does not overlaps with other hints
                var dynamicHint = new DynamicHint()
                {
                    Id = $"DynamicHint{i}",
                    Text = "Welcome\n<b>To HintServiceMeow</b>",
                    TargetX = 0,
                    TargetY = 700,
                };

                player.AddHint(dynamicHint);
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

        public static IEnumerator<float> CoroutineMethod(Player player)
        {
            var hint = new Hint()
            {
                Text = "Hello, this hint will be remove after the last HideAfter is called",
                YCoordinateAlign = HintVerticalAlign.Top,
                YCoordinate = 200,
                FontSize = 60
            };
            player.AddHint(hint);

            //How to use hide after
            //By doing this, the hint will only be hidden 5 seconds after the last HideAfter since every HideAfter method reset the last task.
            //PlayerDisplay.RemoveAfter works the same way
            hint.HideAfter(5f);
            yield return Timing.WaitForSeconds(3f);
            hint.HideAfter(5f);
            yield return Timing.WaitForSeconds(3f);
            hint.HideAfter(5f);
        }
    }
}
