using System;
using System.ComponentModel;
using System.Reflection;
using HarmonyLib;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Patch;
using HintServiceMeow.Integrations;
using HintServiceMeow.UI.Utilities;

//PluginAPI
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

// *    V1.0.0  First Release
// *    V1.0.1
// *        Update the display based on hint's content update
// * ======================================================================================
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
// * ======================================================================================
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
// * ======================================================================================
// *    V4.0.0  Customizable
// *        Add config class for hints
// *        Add refresh event in PlayerDisplay
// *        Add hint Priority
// *        Make common hint customizable
// *        Improve code quality
// * ======================================================================================
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
// *    V5.0.1
// *        Improve experience in font installation
// *        Fix the bug in Dynamic Hint arranging
// *    V5.0.2
// *        Bug fixing
// *    V5.1.0
// *        Add support for \n in text
// *        Improve DynamicHint's performance
// *    V5.1.1
// *        Fix the bug that cause text length to be calculated incorrectly
// *    V5.1.2
// *        Adjust sync speed to improve display performance
// *        Add LineHeight property for all hints
// *    V5.2.0
// *        Add Compatibility Adapter
// *        Improve performance
// *    V5.2.1
// *        Fix the bug that config might not be apply for compatibility adapter
// *    V5.2.2
// *        Bug fixing
// *        Performance improvement
// *        Improve code quality
// *    V5.2.3
// *        Improve compatibility adapter's accuracy. Fix the font size issue
// *    V5.2.4
// *        Add support for color, b, i tags in compatibility adapter.
// *        Add more methods to player display


//TODO: Add support for color, b, i tags in compatibility adapter.

namespace HintServiceMeow
{
#if EXILED
    internal class ExiledPlugin : Exiled.API.Features.Plugin<ExiledPluginConfig>, IPlugin
    {
        //IPlugin
        public PluginType Type => PluginType.Exiled;
        public PluginConfig PluginConfig => this.Config;

        public override string Name => Plugin.Name;
        public override string Author => Plugin.Author;
        public override Version Version => Plugin.Version;

        public override Exiled.API.Enums.PluginPriority Priority => Exiled.API.Enums.PluginPriority.First;

        public override void OnEnabled()
        {
            Plugin.OnEnabled(this);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Plugin.OnDisabled(this);

            base.OnDisabled();
        }

        //IPlugin
        public void BindEvent()
        {
            Exiled.Events.Handlers.Player.Verified += ExiledEventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left += ExiledEventHandler.OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers += ExiledEventHandler.OnWaitingForPlayers;
        }

        public void UnbindEvent()
        {
            Exiled.Events.Handlers.Player.Verified -= ExiledEventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Left -= ExiledEventHandler.OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= ExiledEventHandler.OnWaitingForPlayers;
        }
    }

    internal class ExiledPluginConfig : PluginConfig, Exiled.API.Interfaces.IConfig
    {
    }

    internal static class ExiledEventHandler
    {
        // TryCreate PlayerDisplay and PlayerUIConfig for the new ReferenceHub
        internal static void OnVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (ev.Player.IsNPC || ev.Player.ReferenceHub.isLocalPlayer)
                return;

            PlayerDisplay.TryCreate(ev.Player.ReferenceHub);
            PlayerUI.TryCreate(ev.Player.ReferenceHub);
        }

        internal static void OnLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Destruct(ev.Player.ReferenceHub);
        }

        internal static void OnWaitingForPlayers()
        {
            Patcher.Patch();
        }
    }

#else

    internal class NwapiPlugin : IPlugin
    {
        //IPlugin
        public PluginType Type => PluginType.Exiled;
        public PluginConfig PluginConfig => null;//NW somehow cannot serialize the config for HintServiceMeow

        [PluginEntryPoint("HintServiceMeow", "5.2.4", "A hint framework", "MeowServerOwner")]
        public void LoadPlugin()
        {
            Plugin.OnEnabled(this);
        }

        //IPlugin
        public void BindEvent()
        {
            PluginAPI.Events.EventManager.RegisterEvents<NwapiEventHandler>(this);
        }

        public void UnbindEvent()
        {
            PluginAPI.Events.EventManager.UnregisterEvents(this);
        }
    }

    
    internal class NwapiEventHandler 
    {
        [PluginEvent(ServerEventType.PlayerJoined)]
        internal void OnJoin(PlayerJoinedEvent ev)
        {
            if (ev.Player is null || ev.Player.ReferenceHub.isLocalPlayer || ev.Player.UserId is null)
                return;

            PlayerDisplay.TryCreate(ev.Player.ReferenceHub);
            PlayerUI.TryCreate(ev.Player.ReferenceHub);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        internal void OnLeft(PlayerLeftEvent ev)
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Destruct(ev.Player.ReferenceHub);
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        internal void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            Patcher.Patch();
        }
    }

#endif

    internal static class Plugin
    {
        public static string Name => "HintServiceMeow";
        public static string Author => "MeowServer";
        public static Version Version => new Version(5, 2, 4);

        public static PluginConfig Config = new PluginConfig();//Initialize if fail to initialize

        public static bool HasInitiated = false;

        public static IPlugin ActivePlugin;
        
        public static void OnEnabled(IPlugin plugin)
        {
            if (HasInitiated)
                return;

            HasInitiated = true;
            Config = plugin.PluginConfig ?? Config;
            ActivePlugin = plugin;

            //Register events
            plugin.BindEvent();

            FontTool.LoadFontFile();
            Integrator.StartAllIntegration();

            Log.Info($"HintServiceMeow {Version} has been enabled!");
        }

        public static void OnDisabled(IPlugin plugin)
        {
            //Reset initiated status
            HasInitiated = false;
            Config = null;
            ActivePlugin = null;

            //Unregister events
            plugin.UnbindEvent();

            Patcher.Unpatch();

            PlayerUI.ClearInstance();
            PlayerDisplay.ClearInstance();
        }
    }
}