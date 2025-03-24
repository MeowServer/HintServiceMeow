using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Patch;
using HintServiceMeow.Core.Utilities.Tools;
using HintServiceMeow.UI.Utilities;
using System;

namespace HintServiceMeow
{

#if EXILED
    internal class Plugin : Exiled.API.Features.Plugin<ExiledPluginConfig>
#else
    internal class Plugin
#endif
    {
        public static Plugin Instance;

#if EXILED
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServer";
        public override Version Version => new Version(5, 3, 14);
#else
        [PluginAPI.Core.Attributes.PluginConfig]
        public PluginConfig Config;
#endif

#if EXILED
        public override void OnEnabled()
#else
        [PluginAPI.Core.Attributes.PluginEntryPoint("HintServiceMeow", "5.3.14", "A hint framework", "MeowServer")]
        public void OnEnabled()
#endif
        {
            Instance = this;

#if EXILED
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
#else
            PluginAPI.Events.EventManager.RegisterEvents<Plugin>(this);
#endif

            //Initialize Font Tool
            _ = FontTool.GetCharWidth('a', 40, Core.Enum.TextStyle.Normal);
        }

#if EXILED
        private void OnLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerLeft)]
        private void OnLeft(PluginAPI.Events.PlayerLeftEvent ev)
#endif
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Destruct(ev.Player.ReferenceHub);
        }

#if EXILED
        private void OnWaitingForPlayers()
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.WaitingForPlayers)]
        private void OnWaitingForPlayers(PluginAPI.Events.WaitingForPlayersEvent ev)
#endif

        {
            Patcher.Patch();
        }
    }
}