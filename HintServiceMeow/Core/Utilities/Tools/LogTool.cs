namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used for log messages
    /// </summary>
    internal static class LogTool
    {
        public static void Info(object message) => PluginAPI.Core.Log.Info(message.ToString());
        public static void Error(object message) => PluginAPI.Core.Log.Error(message.ToString());
    }
}
