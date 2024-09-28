using System;

using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Patch;
using HintServiceMeow.UI.Utilities;

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
// *    V5.2.5
// *        Fix the problem that the compatibility adapter's cache might cause high memory usage
// *    V5.3.0 Pre-release 1.0
// *        Add multi-thread support for Core functions
// *        Add pos tag support in compatibility adapter
// *        Add Style component in PlayerUI
// *    V5.3.0 Pre-release 1.1
// *        Fix the bug that cause the color of Style component not working.
// *        Fix the bug that cause Pos tag in compatibility adapter not working.
// *        Support em unit in line-height of compatibility adapter
// *    V5.3.0 Pre-release 1.2
// *        Improve HintParser's behavior
// *        Improve thread safety
// *    V5.3.0 Pre-release 1.3
// *        Fix the bug that cause the compatibility adapter's hints to glitter
// *        Fix the bug that cause multi line hints not displayed correctly
// *    V5.3.0 Pre-release 1.4
// *        Fix the bug that cause dynamic hint to be displayed incorrectly
// *        Fix the bug null reference problem in player display
// *        Fix the bug that cause empty line handled incorrectly 
// *    V5.3.0 Pre-release 2.0
// *        Fix the bug that causes the server to crash when getting the player display
// *        Improve the behavior of the compatibility adaptor
// *        Support size tag in a hint
// *    V5.3.0 Pre-release 2.1
// *        Support for case style and script style tags
// *        Fix the bug that causes the rich text parser to handle the alignment incorrectly
// *        Fix the bug that causes rich text parser break line incorrectly
// *        Add margin properties in the dynamic hint
// *    V5.3.0 Pre-release 2.2
// *        Fix the bug that causes the line height not to be usable
// *        Minor updates and bug fixing
// *    V5.3.0 Pre-release 2.3
// *        Fix the problem that line height was not included when calculating the text height
// *        Fix the problem that font tools does not calculate character length correctly
// *        Fix the problem that rich text parser does not handle line break correctly
// *    V5.3.0
// *        Use 4.8 instead of 4.8.1 as default .net version
// *        Fix the problem that ReceiveHint patch cause crush
// *        Add string builder pool to improve performance
// *        Improve NW API compatibility
// *        Minor naming update
// *    V5.3.1
// *        Add RemoveAfter and HideAfter to PlayerDisplay and AbstractHint
// *        Rewrite update management code in PlayerDisplay
// *    V5.3.2
// *        Fix the bug that cause CompatibilityAdapter to not work correctly
// *        Fix the bug that cause update management to not work correctly

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

        public override void OnReloaded()
        {
            Plugin.OnReloaded();

            base.OnReloaded();
        }

        public void BindEvent()
        {
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public void UnbindEvent()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

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

    internal class ExiledPluginConfig : PluginConfig, Exiled.API.Interfaces.IConfig
    {
    }

#else

    internal class NwapiPlugin : IPlugin
    {
        //IPlugin
        public PluginType Type => PluginType.NwAPI;

        [PluginAPI.Core.Attributes.PluginConfig]
        public PluginConfig Config;
        public PluginConfig PluginConfig => Config;

        [PluginEntryPoint("HintServiceMeow", "5.3.2", "A hint framework", "MeowServer")]
        public void LoadPlugin()
        {
            Plugin.OnEnabled(this);
        }

        //IPlugin
        public void BindEvent()
        {
            PluginAPI.Events.EventManager.RegisterEvents<NwapiPlugin>(this);
        }
         
        public void UnbindEvent()
        {
            PluginAPI.Events.EventManager.UnregisterEvents(this);
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        internal void OnJoin(PlayerJoinedEvent ev)
        {
            if (ev.Player is null 
            || ev.Player.ReferenceHub is null
            || ev.Player.ReferenceHub.isLocalPlayer 
            || string.IsNullOrEmpty(ev.Player.UserId))
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
        public static Version Version => new Version(5, 3, 2);

        public static PluginConfig Config = new PluginConfig();//Default if no config

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

        public static void OnReloaded()
        {
            foreach (var player in PluginAPI.Core.Player.GetPlayers())
            {
                if (player is null 
                    || player.ReferenceHub is null 
                    || player.ReferenceHub.isLocalPlayer 
                    || string.IsNullOrEmpty(player.UserId))
                    continue;

                PlayerDisplay.TryCreate(player.ReferenceHub);
                PlayerUI.TryCreate(player.ReferenceHub);
            }
        }
    }
}