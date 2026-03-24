namespace HintServiceMeow.Plugin
{
    using System.Collections.Generic;
    using System.ComponentModel;

#if EXILED
    using Exiled.API.Interfaces;
#endif

    /// <summary>
    /// Stores configuration settings for the HintServiceMeow plugin.
    /// </summary>
#if EXILED
    internal class PluginConfig : IConfig
#else
    internal class PluginConfig
#endif
    {
        /// <summary>
        /// Gets or sets a value indicating whether the plugin is enabled.
        /// </summary>
        [Description("Indicates whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug mode is enabled. Prints extra logs to the console.
        /// </summary>
        [Description("Indicates whether debug mode is enabled. Prints extra logs to the console.")]
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the hint compatibility adapter is enabled.
        /// </summary>
        [Description("Enabling this feature might make plugins that are incompatible with HintServiceMeow compatible. If the UI of some plugins becomes weird after turning this on, use DisabledCompatAssemblies to block the UI adapter for those plugins.")]
        public bool UseHintCompatibilityAdapter { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of assembly keywords excluded from the compatibility adapter.
        /// </summary>
        [Description("Assemblies that you do not want to be included in the Compatibility Adapter. Use command 'GetCompatAssemblyName' to get the names of all assemblies. Enter keywords (like the assembly name) to ignore them.")]
        public List<string> DisabledCompatAssemblies { get; set; } = new List<string>
        {
            "Some Plugin",
            "Some Other Plugin",
        };

        /// <summary>
        /// Gets or sets the default display time in seconds for an item hint with a description.
        /// </summary>
        [Description("The default time (in seconds) to show an Item hint. 'Short' means the hint has a title but no description.")]
        public int ItemHintDisplayTime { get; set; } = 10;

        /// <summary>
        /// Gets or sets the default display time in seconds for a short item hint (title only).
        /// </summary>
        public int ShortItemHintDisplayTime { get; set; } = 5;

        /// <summary>
        /// Gets or sets the default display time in seconds for a map hint.
        /// </summary>
        [Description("The default time (in seconds) to show a Map hint.")]
        public int MapHintDisplayTime { get; set; } = 10;

        /// <summary>
        /// Gets or sets the default display time in seconds for a short map hint.
        /// </summary>
        public int ShortMapHintDisplayTime { get; set; } = 7;

        /// <summary>
        /// Gets or sets the default display time in seconds for a role hint.
        /// </summary>
        [Description("The default time (in seconds) to show a Role hint.")]
        public int RoleHintDisplayTime { get; set; } = 15;

        /// <summary>
        /// Gets or sets the default display time in seconds for a short role hint.
        /// </summary>
        public int ShortRoleHintDisplayTime { get; set; } = 5;

        /// <summary>
        /// Gets or sets the default display time in seconds for other common hints.
        /// </summary>
        [Description("The default time (in seconds) to show other common hints.")]
        public int OtherHintDisplayTime { get; set; } = 5;
    }
}
