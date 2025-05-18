namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used for log messages
    /// </summary>
    internal static class LogTool
    {
#if DEBUG
        public static void Info(object message)
        {
            LabApi.Features.Console.Logger.Info(message.ToString());
        }
#endif
        public static void Error(object message)
        {
            LabApi.Features.Console.Logger.Error(message.ToString());
        }
    }
}
