using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Patch;
using HintServiceMeow.Core.Utilities.Tools;
using HintServiceMeow.UI.Utilities;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader;
using System;

namespace HintServiceMeow
{

#if EXILED
    internal class Plugin : Exiled.API.Features.Plugin<ExiledPluginConfig>
#else
    internal class Plugin : LabApi.Loader.Features.Plugins.Plugin
#endif
    {
        public static Plugin Instance;

#if EXILED
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServer";
        public override Version Version => new Version(5, 4, 0);
#else
        public override string Name => "HintServiceMeow";
        public override string Author => "MeowServer";
        public override Version Version => new(5, 4, 0);
        public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);
        public override string Description => "A hint framework";

        public PluginConfig Config;
        public override void LoadConfigs()
        {
            base.LoadConfigs();

            Config = this.LoadConfig<PluginConfig>("config.yml");
        }
#endif

#if EXILED
        public override void OnEnabled()
#else
        public override void Enable()
#endif
        {
            Instance = this;

#if EXILED
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
#else
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            PlayerEvents.Left += OnLeft;
#endif

            //Initialize Font Tool
            _ = FontTool.GetCharWidth('a', 40, Core.Enum.TextStyle.Normal);
        }

#if EXILED
        private void OnLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
#else
        private void OnLeft(PlayerLeftEventArgs ev)
#endif
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Destruct(ev.Player.ReferenceHub);
        }

        private void OnWaitingForPlayers()

        {
            Patcher.Patch();
        }
#if EXILED
        
#else
        public override void Disable()
        {
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            PlayerEvents.Left -= OnLeft;
        }
    }
#endif
}