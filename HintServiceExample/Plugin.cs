using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;
using LabApi.Events.Arguments.PlayerEvents;
using System;
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace HintServiceExample
{
    /// <summary>
    /// This is an Exiled only example of how to create a simple ui for players using Hint and PlayerDisplay.
    /// </summary>
    public class Plugin : LabApi.Loader.Features.Plugins.Plugin
    {
        public override string Name => "HintServiceExample";
        public override string Author => "MeowServer";
        public override string Description => "A example plugin for HSM";
        public override Version Version { get; } = new Version(5, 4, 3);
        public override Version RequiredApiVersion { get; } = Version.Parse(LabApi.Features.LabApiProperties.CompiledVersion);
        public override void Enable()
        {
            LabApi.Events.Handlers.PlayerEvents.Joined += EventHandler.OnVerified;
            LabApi.Events.Handlers.ServerEvents.WaitingForPlayers += EventHandler.OnWaitingForPlayer;
        }

        public override void Disable()
        {
            LabApi.Events.Handlers.PlayerEvents.Joined -= EventHandler.OnVerified;
            LabApi.Events.Handlers.ServerEvents.WaitingForPlayers -= EventHandler.OnWaitingForPlayer;
        }
    }

    public static class EventHandler
    {
        public static void OnWaitingForPlayer()
        {
            //To better demonstrate the hint, we will hide the lobby timer
            GameObject.Find("StartRound").transform.localScale = Vector3.zero;
        }

        public static void OnVerified(PlayerJoinedEventArgs ev)
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
