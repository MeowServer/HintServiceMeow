using System;
using System.Collections.Generic;
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
        public override Version Version => new Version(5, 4, 2);
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
            ev.Player.SendHint("Hello, world! (3s)", 3f);

            Timing.CallDelayed(2f, () => {
                ev.Player.SendHint("Second hint! (3s)", 10f);
            });

            Timing.CallDelayed(5f, () => {
                ev.Player.SendHint("", 0f); // duration为0表示立即清空
            });

            Timing.CallDelayed(10f, () => {
                for (int i = 0; i < 5; i++)
                {
                    float delay = i * 1.5f;
                    Timing.CallDelayed(delay, () => {
                        ev.Player.SendHint($"Hint #{i + 1} (1.2s)", 1.2f);
                    });
                }
            });
        }
    }
}
