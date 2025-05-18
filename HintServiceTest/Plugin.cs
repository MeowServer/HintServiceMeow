using System;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;
using MEC;

namespace HintServiceTest
{
    /// <summary>
    /// This plugin is made to test the HintServiceMeow.
    /// </summary>
    public class Plugin : LabApi.Loader.Features.Plugins.Plugin
    {
        public override string Name => "HintServiceTest";
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
            var pd = PlayerDisplay.Get(ev.Player);
            Hint hint = new Hint
            {
                Text = "Hello HSM!",
                YCoordinate = 100f
            };
            pd.AddHint(hint);
            pd.RemoveAfter(hint, 10f);

            Timing.CallDelayed(5f, () =>
            {
                ev.Player.ReferenceHub.hints.Show(new Hints.TextHint("Hello World!"));
            });
            Timing.CallDelayed(10f, () =>
            {
                ev.Player.SendHint("Hello World LabAPI!", 10f);
            });
            //Exiled.API.Features.Player.Get(ev.Player.ReferenceHub).ShowHint("Hello World Exiled!\n\n", 10f);
        }
    }
}
