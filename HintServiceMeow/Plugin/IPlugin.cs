namespace HintServiceMeow
{
    internal interface IPlugin
    {
        Plugin.PluginType Type { get; }

        PluginConfig PluginConfig { get; }

        void BindEvent();

        void UnbindEvent();
    }
}
