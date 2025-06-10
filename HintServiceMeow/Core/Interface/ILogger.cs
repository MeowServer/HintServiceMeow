namespace HintServiceMeow.Core.Interface
{
    public interface ILogger
    {
        void Info(object message);
        void Error(object message);
    }
}
