namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used for log messages
    /// </summary>
    internal static class LogTool
    {
#if DEBUG
        public static void Info(object message) => PluginAPI.Core.Log.Info(message.ToString());
#endif
        public static void Error(object message) => PluginAPI.Core.Log.Error(message.ToString());
    }
}
