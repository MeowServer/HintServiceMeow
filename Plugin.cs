using System;

using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Enums;
using Hints;
using HarmonyLib;


//*V1.0.0
//* V1.0.1
//* Update the display based on hint's updation
// * V2.0.0
// *      Support Dynamic Hint
// *      Limit maximum update rate to 0.5/second
// *      Fixed some bugs     
// *V2.1.0
// *      Add Common Hints
// *V2.1.1
// *      Fix some bugs
// *v2.2.0
// *      Use the event to update the player display, increase stability, and decrease costs
// *v3.0.0
// *      Player UI is separated from PlayerDisplay and extended for more methods
// *v3.0.1
// *      Fix some bugs

namespace HintServiceMeow
{
    class Plugin : Plugin<Config>
    {
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServerOwner";
        public override Version Version => new Version(3, 0, 1);

        public override PluginPriority Priority => PluginPriority.First;

        public static Plugin instance;

        public static Harmony _harmony;

        public override void OnEnabled()
        {
            instance = this;
            Config.instance = Config;

            _harmony = new Harmony("HintServiceMeowHarmony");
            _harmony.PatchAll();

            Exiled.Events.Handlers.Player.Verified += EventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left += EventHandler.OnLeft;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            instance = null;
            Config.instance = null;

            _harmony.UnpatchAll();
            _harmony = null;

            Exiled.Events.Handlers.Player.Verified -= EventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left -= EventHandler.OnLeft;

            EventHandler.OnDisable();

            base.OnDisabled();
        }
    }

    public static class EventHandler
    {
        public delegate void NewPlayerHandler(PlayerDisplay playerDisplay);
        public static event NewPlayerHandler NewPlayer;

        /// <summary>
        /// Create PlayerDisplay and PlayerUI for the new player
        /// </summary>
        /// <param name="ev"></param>
        private static void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNPC) return;

            var pd = new PlayerDisplay(ev.Player);
            new PlayerUI(ev.Player);

            NewPlayer.Invoke(pd);
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            PlayerUI.RemovePlayerUI(ev.Player);
            PlayerDisplay.RemovePlayerDisplay(ev.Player);
        }

        private static void OnDisable()
        {
            PlayerUI.ClearAllPlayerUI();
            PlayerDisplay.ClearPlayerDisplay();
        }
    }
}
