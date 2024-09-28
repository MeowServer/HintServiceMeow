namespace HintServiceMeow
{
    internal interface IPlugin
    {
        PluginType Type { get; }

        PluginConfig PluginConfig { get; }

        void BindEvent();

        void UnbindEvent();
    }
}
