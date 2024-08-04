using System;
using System.Reflection;

using HarmonyLib;
using HintService.Config;
using HintService.Core.Utilities;
using HintService.UI.Utilities;

//PluginAPI
using PluginAPI.Core.Attributes;

// *    V1.0.0  First Release
// *    V1.0.1
// *        Update the display based on hint's content update
// *
// *    V2.0.0  Dynamic Hint
// *        Support Dynamic Hint
// *        Limit maximum update rate to 0.5/second
// *        Fixed some bugs     
// *    V2.1.0
// *        Add Common Hints
// *    V2.1.1
// *        Fix some bugs
// *    V2.2.0
// *        Use the event to update the ReferenceHub display, increase stability, and decrease costs
// *
// *    V3.0.0  ReferenceHub UI
// *        ReferenceHub UI is separated from PlayerDisplay and extended for more methods
// *    V3.0.1
// *        Fix some bugs
// *    V3.0.2
// *        Fix the bug that crush the PlayerDisplay when there's no hint displayed on the screen
// *    V3.1.0
// *        Add PlayerUIConfig Config
// *        TODO: ADD configs for spectator template, scp extra info, spectator info, etc.
// *    V3.1.1
// *        bug fixing
// *    V3.1.2
// *        Use patch to block all the hints from other plugins
// *    V3.2.0
// *        Organized config
// *        Make PlayerUIConfig more customizable
// *    V3.3.0
// *        Separate PlayerUITemplate from PlayerUIConfig
// *        PlayerUITemplate is now a new plugin called CustomizableUIMeow
// *
// *    V4.0.0  Customizable
// *        Add config class for hints
// *        Add refresh event in PlayerDisplay
// *        Add hint Priority
// *        Make common hint customizable
// *        Improve code quality
// *    V5.0.0  Rework
// *        Rewrite core code
// *        Add sync speed and auto text and several new properties to hint
// *        Standardized code style
// *        Add NW API support
// *        Remove hint config templates
// *        Separate PlayerUI and CommonHint

namespace HintService
{
    using PluginConfig = Config.PluginConfig;

    internal class ExiledPlugin : Exiled.API.Features.Plugin<ExiledPluginConfig>
    {
        public override string Name => Plugin.Name;
        public override string Author => Plugin.Author;
        public override Version Version => Plugin.Version;

        public override Exiled.API.Enums.PluginPriority Priority => Exiled.API.Enums.PluginPriority.First;

        public override void OnEnabled()
        {
            Plugin.OnEnabled(this.Config);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Plugin.OnDisabled();

            base.OnDisabled();
        }
    }

    internal class NwapiPlugin
    {
        [PluginAPI.Core.Attributes.PluginConfig]
        public PluginConfig Config;

        public static NwapiPlugin Instance;

        [PluginEntryPoint("HintServiceMeow", "4.0.0", "A hint framework", "MeowServerOwner")]
        public void LoadPlugin()
        {
            if (!Config.IsEnabled)
                return;

            Instance = this;

            Plugin.OnEnabled(Config);
        }
    }

    internal static class Plugin
    {
        public static string Name => "HintServiceMeow";
        public static string Author => "MeowServer";
        public static Version Version => new Version(5, 0, 0);

        public static IPluginConfig Config;

        private static Harmony _harmony;

        private static bool _hasInitiated = false;
        
        public static void OnEnabled(IPluginConfig config)
        {
            //Check initiated status
            if (_hasInitiated)
                return;

            //Set initiated status
            _hasInitiated = true;

            Config = config;

            _harmony = new Harmony("HintServiceMeowHarmony" + Version);
            _harmony.PatchAll();

            //Register events
            if (!(Assembly.GetExecutingAssembly().GetType("Exiled.Events.Handlers.Player") != null))
            {
                Exiled.Events.Handlers.Player.Verified += ExiledEventHandler.OnVerified;
                Exiled.Events.Handlers.Player.Left += ExiledEventHandler.OnLeft;
            }
            else
            {
                PluginAPI.Events.EventManager.RegisterEvents<NwapiEventHandler>(NwapiPlugin.Instance);
            }
        }

        public static void OnDisabled()
        {
            //Reset initiated status
            _hasInitiated = false;

            Config = null;

            _harmony.UnpatchAll();
            _harmony = null;

            //Unregister events
            if (!(Assembly.GetExecutingAssembly().GetType("Exiled.Events.Handlers.Player") is null))
            {
                Exiled.Events.Handlers.Player.Verified -= ExiledEventHandler.OnVerified;
                Exiled.Events.Handlers.Player.Left -= ExiledEventHandler.OnLeft;
            }

            PluginAPI.Events.EventManager.UnregisterAllEvents(NwapiPlugin.Instance);

            PlayerUI.ClearInstance();
            PlayerDisplay.ClearInstance();
        }
    }
}


