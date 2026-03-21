namespace HintServiceMeow.Core.Interface
{
    using System;
    using HintServiceMeow.Core.Enum;

    internal interface ITaskScheduler : IDisposable
    {
        public TimeSpan Elapsed { get; }

        public TimeSpan MinInterval { get; set; }

        public bool InvokeUntilSuccess { get; set; }

        public bool IsReadyForNextAction { get; }

        void Start(TimeSpan interval, Action callback);

        void Start(TimeSpan interval, Func<bool> callback);

        void Invoke(float delay = -1f, DelayType delayType = DelayType.Override);

        void Stop();

        void Pause();

        void Resume();
    }
}
