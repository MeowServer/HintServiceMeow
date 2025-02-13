using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;

namespace HintServiceTest
{
    /// <summary>
    /// This is the plugin I made to test the HintServiceMeow.
    /// </summary>
    public class Plugin : LabApi.Loader.Features.Plugins.Plugin
    {
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServer";
        public override Version Version => new Version(5, 4, 0);
        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);
        public override string Description => "A hint framework";

        public override void Enable()
        {
            // Add code to handle when the plugin is enabled
            LabApi.Events.Handlers.PlayerEvents.Joined += EventHandler.OnJoined;
        }

        public override void Disable()
        {
            // Add code to handle when the plugin is disabled
            LabApi.Events.Handlers.PlayerEvents.Joined -= EventHandler.OnJoined;

        }
    }

    public static class EventHandler
    {
        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            ev.Player.SendHint("Hello world", 10f);
        }
    }
}
