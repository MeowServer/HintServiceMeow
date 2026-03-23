namespace HintServiceMeow.Plugin
{
    using System;
    using HintServiceMeow.Core.Utilities;
    using HintServiceMeow.Core.Utilities.Patch;
    using HintServiceMeow.Core.Utilities.Tools;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;
    using HintServiceMeow.UI.Utilities;

#if !EXILED
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Loader;
    using LabApi.Loader.Features.Plugins.Enums;
#endif

#if EXILED
    internal class Plugin : Exiled.API.Features.Plugin<PluginConfig>
#else
    /// <summary>
    /// Represents the main plugin class for the HintServiceMeow framework.
    /// </summary>
    internal class Plugin : LabApi.Loader.Features.Plugins.Plugin
#endif
    {
        /// <summary>
        /// Gets the singleton instance of the Plugin class.
        /// </summary>
        public static Plugin Instance { get; private set; } = null!;

#if EXILED
        public override string Name => "HintServiceMeow";

        public override string Author => "MeowServer";

        public override Version Version => new(6, 0, 0);

        public override Version RequiredExiledVersion => new(9, 6, 0);

        public override Exiled.API.Enums.PluginPriority Priority => Exiled.API.Enums.PluginPriority.Highest;
#else
        /// <summary>
        /// Gets the display name of the plugin.
        /// </summary>
        public override string Name => "HintServiceMeow";

        /// <summary>
        /// Gets the name of the author of the server implementation.
        /// </summary>
        public override string Author => "MeowServer";

        /// <summary>
        /// Gets the version of the framework.
        /// </summary>
        public override Version Version => new(6, 0, 0);

        /// <summary>
        /// Gets the minimum LabAPI version required for compatibility with this implementation.
        /// </summary>
        public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

        /// <summary>
        /// Gets a brief description of the framework.
        /// </summary>
        public override string Description => "A hint framework";

        /// <summary>
        /// Gets the load priority for this resource.
        /// </summary>
        public override LoadPriority Priority => LoadPriority.Highest;

        /// <summary>
        /// Gets the configuration settings for the plugin.
        /// </summary>
        public PluginConfig Config { get; private set; } = null!;

        /// <summary>
        /// Loads the plugin configuration from the default configuration file and assigns it to the Config property.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown if the configuration file cannot be loaded or is missing required data.</exception>
        public override void LoadConfigs()
        {
            base.LoadConfigs();

            Config = this.LoadConfig<PluginConfig>("config.yml") ?? throw new NullReferenceException("Could not load plugin config!");
        }
#endif

#if EXILED
        public override void OnEnabled()
#else
        /// <summary>
        /// Enables the plugin and subscribes to relevant server and player events required for its operation.
        /// </summary>
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

            // Initialize Components
            _ = FontTool.Instance;
            _ = ConcurrentTaskDispatcher.Instance;

            NetworkTimeCache.Initialize(new UnityCoroutineRunner());
#if EXILED
            base.OnEnabled();
#endif
        }

#if EXILED
        public override void OnDisabled()
#else
        /// <summary>
        /// Disables the plugin and unregisters event handlers.
        /// </summary>
        public override void Disable()
#endif
        {
#if EXILED
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
#else
            PlayerEvents.Left -= OnLeft;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
#endif

#if EXILED
            base.OnDisabled();
#endif
        }

        /// <summary>
        /// Initializes patching operations required while waiting for players to join.
        /// </summary>
        private static void OnWaitingForPlayers()
        {
            Patcher.Patch();
        }

#if EXILED
        private static void OnLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
#else
        private void OnLeft(PlayerLeftEventArgs ev)
#endif
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Dispose(ev.Player.ReferenceHub);
        }
    }
}