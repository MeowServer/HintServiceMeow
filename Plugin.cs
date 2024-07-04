using System;

using Exiled.API.Features;
using Exiled.API.Enums;
using HarmonyLib;
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
// *         Add PlayerUIConfig Config
// *        TODO: ADD configs for spectator template, scp extra info, spectator info, etc.
// *    V3.1.1
// *        bug fixing
// *    V3.1.2
// *         Use patch to block all the hints from other plugins
// *    V3.2.0
// *         Organized config
// *         Make PlayerUIConfig more customizable
// *    V3.3.0
// *         Separate PlayerUITemplate from PlayerUIConfig
// *         PlayerUITemplate is now a new plugin called CustomizableUIMeow
// *    V4.0.0
// *        Add config for hints
// *        Improve code quality
// *        Add more comments

namespace HintServiceMeow
{
    internal class Plugin : Plugin<PluginConfig>
    {
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServerOwner";
        public override Version Version => new Version(3, 3, 0);

        public override PluginPriority Priority => PluginPriority.First;

        public static Plugin instance;

        public static Harmony _harmony;

        public override void OnEnabled()
        {
            instance = this;
            PluginConfig.Instance = Config;

            _harmony = new Harmony("HintServiceMeowHarmony");
            _harmony.PatchAll();

            Exiled.Events.Handlers.Player.Verified += EventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left += EventHandler.OnLeft;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            instance = null;
            PluginConfig.Instance = null;

            _harmony.UnpatchAll();
            _harmony = null;

            Exiled.Events.Handlers.Player.Verified -= EventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left -= EventHandler.OnLeft;

            EventHandler.OnDisable();

            base.OnDisabled();
        }
    }

    
}
