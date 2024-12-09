using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Patch;
using HintServiceMeow.Core.Utilities.Tools;
using HintServiceMeow.UI.Utilities;
using System;

namespace HintServiceMeow
{
#if EXILED
    internal class ExiledPlugin : Exiled.API.Features.Plugin<ExiledPluginConfig>
    {
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServer";
        public override Version Version => new Version(5, 3, 9);

        public override Exiled.API.Enums.PluginPriority Priority => Exiled.API.Enums.PluginPriority.First;

        public override void OnEnabled()
        {
            Plugin.OnEnabled(new NWAPIPlugin(), this.Config);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Plugin.OnDisabled(new NWAPIPlugin());

            base.OnDisabled();
        }
    }
#endif

    internal class NWAPIPlugin
    {
        [PluginAPI.Core.Attributes.PluginConfig]
        public PluginConfig Config;

        [PluginAPI.Core.Attributes.PluginEntryPoint("HintServiceMeow", "5.3.9", "A hint framework", "MeowServer")]
        public void LoadPlugin()
        {
            Plugin.OnEnabled(this, Config);
        }

        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerLeft)]
        internal void OnLeft(PluginAPI.Events.PlayerLeftEvent ev)
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Destruct(ev.Player.ReferenceHub);
        }

        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.WaitingForPlayers)]
        internal void OnWaitingForPlayers(PluginAPI.Events.WaitingForPlayersEvent ev)
        {
            Patcher.Patch();
        }
    }

    internal static class Plugin
    {
        public static bool HasInitiated = false;

        public static PluginConfig Config;//Default if there's no config
        public static NWAPIPlugin PluginInstance;

        public static void OnEnabled(NWAPIPlugin plugin, PluginConfig config)
        {
            if (HasInitiated)
                return;

            HasInitiated = true;
            PluginInstance = plugin;
            Config = config;

            PluginAPI.Events.EventManager.RegisterEvents<NWAPIPlugin>(plugin);

            //Initialize Font Tool
            FontTool.GetCharWidth('a', 40, Core.Enum.TextStyle.Normal);
        }

        public static void OnDisabled(NWAPIPlugin plugin)
        {
            HasInitiated = false;
            PluginInstance = null;
            Config = null;

            PluginAPI.Events.EventManager.UnregisterEvents(plugin);

            Patcher.Unpatch();
        }
    }
}