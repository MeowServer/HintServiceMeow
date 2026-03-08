namespace HintServiceMeow.Plugin
{
    using System.Collections.Generic;
    using System.ComponentModel;

#if EXILED
    using Exiled.API.Interfaces;
#endif

#if EXILED
    internal class PluginConfig : IConfig
#else
    internal class PluginConfig
#endif
    {
        [Description("Indicates whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Indicates whether debug mode is enabled. Prints extra logs to the console.")]
        public bool Debug { get; set; } = false;

        [Description("Enabling this feature might make plugins that are incompatible with HintServiceMeow compatible. If the UI of some plugins becomes weird after turning this on, use DisabledCompatAssemblies to block the UI adapter for those plugins.")]
        public bool UseHintCompatibilityAdapter { get; set; } = false;

        [Description("Assemblies that you do not want to be included in the Compatibility Adapter. Use command 'GetCompatAssemblyName' to get the names of all assemblies. Enter keywords (like the assembly name) to ignore them.")]
        public List<string> DisabledCompatAssemblies { get; set; } = new List<string>
        {
            "Some Plugin",
            "Some Other Plugin",
        };

        [Description("The default time (in seconds) to show an Item hint. 'Short' means the hint has a title but no description.")]
        public int ItemHintDisplayTime { get; set; } = 10;

        public int ShortItemHintDisplayTime { get; set; } = 5;

        [Description("The default time (in seconds) to show a Map hint.")]
        public int MapHintDisplayTime { get; set; } = 10;

        public int ShortMapHintDisplayTime { get; set; } = 7;

        [Description("The default time (in seconds) to show a Role hint.")]
        public int RoleHintDisplayTime { get; set; } = 15;

        public int ShortRoleHintDisplayTime { get; set; } = 5;

        [Description("The default time (in seconds) to show other common hints.")]
        public int OtherHintDisplayTime { get; set; } = 5;
    }
}