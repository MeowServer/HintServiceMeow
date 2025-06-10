using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used for log messages
    /// </summary>
    public class Logger : ILogger
    {
        public static ILogger Instance { get; set; } = new Logger();

        public void Info(object message)
        {
            LabApi.Features.Console.Logger.Info(message.ToString());
        }

        public void Error(object message)
        {
            LabApi.Features.Console.Logger.Error(message.ToString());
        }
    }
}
