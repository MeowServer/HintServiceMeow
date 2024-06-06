using System;

using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Enums;
using Hints;
using HarmonyLib;
using HintServiceMeow;
using HintServiceMeow.Config;


// *    V1.0.0
// *    V1.0.1
// *        Update the display based on hint's updation
// *    V2.0.0
// *        Support Dynamic Hint
// *        Limit maximum update rate to 0.5/second
// *        Fixed some bugs     
// *    V2.1.0
// *        Add Common Hints
// *    V2.1.1
// *        Fix some bugs
// *    V2.2.0
// *        Use the event to update the player display, increase stability, and decrease costs
// *    V3.0.0
// *         Player UI is separated from PlayerDisplay and extended for more methods
// *    V3.0.1
// *        Fix some bugs
// *    V3.0.2
// *        Fix the bug that crush the PlayerDisplay when there's no hint displayed on the screen
// *    V3.1.0
// *         Add PlayerUI Config
// *        TODO: ADD configs for spectator template, scp extra info, spectator info, etc.
// *    V3.1.1
// *        bug fixing
// *    V3.1.2
// *         Use patch to block all the hints from other plugins
// *    V3.2.0
// *         Organized config
// *        Make PlayerUI more customizable

namespace HintServiceMeow
{
    class Plugin : Plugin<PluginConfig>
    {
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServerOwner";
        public override Version Version => new Version(3, 2, 0);

        public override PluginPriority Priority => PluginPriority.First;

        public static Plugin instance;

        public static Harmony _harmony;

        public override void OnEnabled()
        {
            instance = this;
            PluginConfig.instance = Config;

            _harmony = new Harmony("HintServiceMeowHarmony");
            _harmony.PatchAll();

            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Left += OnLeft;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            instance = null;
            PluginConfig.instance = null;

            _harmony.UnpatchAll();
            _harmony = null;

            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.Left -= OnLeft;

            OnDisable();

            base.OnDisabled();
        }

        // Create PlayerDisplay and PlayerUI for the new player
        private static void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNPC) return;

            var pd = new PlayerDisplay(ev.Player);
            if(PluginConfig.instance.EnablePlayerUI)
                new PlayerUI(ev.Player);

            EventHandler.InvokeNewPlayerEvent(pd);
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            PlayerUI.RemovePlayerUI(ev.Player);
            PlayerDisplay.RemovePlayerDisplay(ev.Player);
        }

        private static void OnDisable()
        {
            PlayerUI.ClearPlayerUI();
            PlayerDisplay.ClearPlayerDisplay();
        }
    }

    public static class EventHandler
    {
        public delegate void NewPlayerHandler(PlayerDisplay playerDisplay);
        public static event NewPlayerHandler NewPlayer;

        internal static void InvokeNewPlayerEvent(PlayerDisplay pd)
        {
            NewPlayer?.Invoke(pd);
        }
    }
}
