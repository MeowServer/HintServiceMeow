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

        private NwapiPlugin PluginInstance = new NwapiPlugin();

        public override void OnEnabled()
        {
            Plugin.OnEnabled(PluginInstance, this.Config);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Plugin.OnDisabled(PluginInstance);

            base.OnDisabled();
        }
    }
#endif

    internal class NwapiPlugin
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

        public static void OnEnabled(NwapiPlugin plugin, PluginConfig config)
        {
            if (HasInitiated)
                return;

            HasInitiated = true;
            Config = config;

            PluginAPI.Events.EventManager.RegisterEvents<NwapiPlugin>(plugin);

            //Initialalize Font Tool
            FontTool.GetCharWidth('a', 40, Core.Enum.TextStyle.Normal);
        }

        public static void OnDisabled(NwapiPlugin plugin)
        {
            HasInitiated = false;
            Config = null;

            PluginAPI.Events.EventManager.UnregisterEvents(plugin);

            Patcher.Unpatch();
        }
    }
}