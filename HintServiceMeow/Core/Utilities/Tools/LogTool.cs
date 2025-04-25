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
            MainThreadDispatcher.Dispatch(() => LabApi.Features.Console.Logger.Info(message.ToString()));
        }
#endif
        public static void Error(object message)
        {
            //Log class is thread safe, but we use MainThreadDispatcher to ensure that.
            MainThreadDispatcher.Dispatch(() => LabApi.Features.Console.Logger.Error(message.ToString()));
        }
    }
}
