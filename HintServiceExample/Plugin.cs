using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;
using UnityEngine;
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
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayer;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= EventHandler.OnVerified;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayer;

            base.OnDisabled();
        }

        public void OnWaitingForPlayer()
        {
            //To better demonstrate the hint, we will hide the lobby timer
            GameObject.Find("StartRound").transform.localScale = Vector3.zero;
        }
    }

    public static class EventHandler
    {
        public static void OnVerified(VerifiedEventArgs ev)
        {
            Hint hint = new Hint
            {
                Text = "Hello World"
            };

            hint.FontSize = 40;
            hint.YCoordinate = 700;
            hint.Alignment = HintAlignment.Left;

            PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
            playerDisplay.AddHint(hint);

            DynamicHint dynamicHint = new DynamicHint
            {
                Text = "Hello Dynamic Hint"
            };

            playerDisplay.AddHint(dynamicHint);

            PlayerUI ui = PlayerUI.Get(ev.Player);
            ui.CommonHint.ShowRoleHint("SCP173", new[] { "Kill all humans", "Use your skills" });
            ui.CommonHint.ShowMapHint("Heavy Containment Zone", "The place where most SCPs spawn");
            ui.CommonHint.ShowItemHint("Keycard", "Used to open doors");
            ui.CommonHint.ShowOtherHint("The server is starting!");
        }
    }
}
