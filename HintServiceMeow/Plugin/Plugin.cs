using System;
using System.Reflection;

using HarmonyLib;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;
using PluginAPI.Core;

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
// *    V5.0.0  Rework(Pre-release)
// *        Rewrite core code
// *        Add sync speed, auto text, and several new properties to hint
// *        Standardized code style
// *        Add NW API support
// *        Remove hint config template
// *        Separate PlayerUI and CommonHint
// *    V5.0.0  Rework
// *        Fix the bug that cause the font file to place in the TEMP folder
// *        Fix the bug that NW API cannot load this plugin correctly

namespace HintServiceMeow
{
    internal class ExiledPlugin : Exiled.API.Features.Plugin<ExiledPluginConfig>
    {
        public override string Name => Plugin.Name;
        public override string Author => Plugin.Author;
        public override Version Version => Plugin.Version;

        public override Exiled.API.Enums.PluginPriority Priority => Exiled.API.Enums.PluginPriority.First;

        public override void OnEnabled()
        {
            Plugin.OnEnabled(this.Config, true);

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
        public static NwapiPlugin Instance;

        [PluginEntryPoint("HintServiceMeow", "5.0.0", "A hint framework", "MeowServerOwner")]
        public void LoadPlugin()
        {
            Instance = this;

            Plugin.OnEnabled(null, false);
        }
    }

    internal static class Plugin
    {
        public static string Name => "HintServiceMeow";
        public static string Author => "MeowServer";
        public static Version Version => new Version(5, 0, 0);

        public static PluginConfig Config = new PluginConfig();//Initialize if fail to initialize

        private static Harmony _harmony;

        private static bool _hasInitiated = false;
        
        public static void OnEnabled(PluginConfig config, bool isExiled)
        {
            //Check initiated status
            if (_hasInitiated)
                return;

            //Set initiated status
            _hasInitiated = true;

            if(config != null)
                Config = config;

            _harmony = new Harmony("HintServiceMeowHarmony" + Version);
            _harmony.PatchAll();

            FontTool.CheckFontFile();

            //Register events
            if (isExiled)
            {
                RegisterExiledEvent();
            }
            else
            {
                PluginAPI.Events.EventManager.RegisterEvents<NwapiEventHandler>(NwapiPlugin.Instance);
            }

            Log.Info("HintServiceMeow has been enabled!");
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
                UnregisterExiledEvent();
            }

            PluginAPI.Events.EventManager.UnregisterAllEvents(NwapiPlugin.Instance);

            PlayerUI.ClearInstance();
            PlayerDisplay.ClearInstance();
        }

        private static void RegisterExiledEvent()
        {
            Exiled.Events.Handlers.Player.Verified += ExiledEventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left += ExiledEventHandler.OnLeft;
        }

        private static void UnregisterExiledEvent()
        {
            Exiled.Events.Handlers.Player.Verified -= ExiledEventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left -= ExiledEventHandler.OnLeft;
        }
    }
}