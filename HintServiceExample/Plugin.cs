using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.UI.Extension;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;
using MEC;

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
            var nameHint = new Hint()
            {
                Text = $"Hello, {ev.Player.Nickname}",
                YCoordinateAlign = HintVerticalAlign.Top,
                YCoordinate = 0,
                Alignment = HintAlignment.Left,
                FontSize = 20
            };

            var currentTimeHint = new Hint()
            {
                AutoText = GetCurrentTime,
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

            var pd = ev.Player.GetPlayerDisplay();
            pd.AddHint(nameHint);
            pd.AddHint(currentTimeHint);
            pd.AddHint(tPSHint);

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

            var dynamicHint = new DynamicHint()
            {
                Text = "Welcome To HintServiceMeow",
                TargetX = 0,
                TargetY = 700,
            };

            var blocker = new Hint()
            {
                Text = "Hope you enjoy it!",
                XCoordinate = 0,
                YCoordinate = 700,
                FontSize = 40,
            };

            Timing.CallDelayed(10f, () =>
            {
                pd.RemoveHint(blocker);
            });

            pd.AddHint(dynamicHint);
            pd.AddHint(blocker);
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
