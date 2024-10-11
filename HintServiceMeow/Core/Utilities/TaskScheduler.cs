using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

using PluginAPI.Core;
using MEC;

namespace HintServiceMeow.Core.Utilities
{
    internal class TaskScheduler
    {
        private readonly Action _action;

        public readonly TimeSpan Interval;
        private readonly CoroutineHandle _actionCoroutine;
        private readonly ReaderWriterLockSlim _coroutineLock = new ReaderWriterLockSlim();

        public Stopwatch LastActionStopwatch { get; private set; } = Stopwatch.StartNew();
        public DateTime NextActionTime { get; private set; } = new DateTime();
        private readonly ReaderWriterLockSlim _actionTimeLock = new ReaderWriterLockSlim();

        public TaskScheduler(TimeSpan interval, Action action)
        {
            this.Interval = interval;
            this._action = action ?? throw new ArgumentNullException(nameof(action));

            //Force change the elapsed time of the stopwatch
            //This is evil......
            _actionTimeLock.EnterWriteLock();
            try
            {
                var field = typeof(Stopwatch).GetField("elapsed", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(LastActionStopwatch, interval.Ticks);
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }

            _coroutineLock.EnterWriteLock();
            try
            {
                this._actionCoroutine = Timing.RunCoroutine(TaskCoroutineMethod());
            }
            finally
            {
                _coroutineLock.ExitWriteLock();
            }

        }

        public void StartAction()
        {
            _actionTimeLock.EnterWriteLock();

            try
            {
                NextActionTime = DateTime.MinValue;
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }
        }

        public void StartAction(float delay, DelayType delayType)
        {
            _actionTimeLock.EnterWriteLock();

            try
            {
                if (NextActionTime == DateTime.MaxValue)
                {
                    NextActionTime = DateTime.Now.AddSeconds(delay);
                    return;
                }

                switch (delayType)
                {
                    case DelayType.Fastest:
                        if (NextActionTime > DateTime.Now.AddSeconds(delay))
                            NextActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.Latest:
                        if (NextActionTime < DateTime.Now.AddSeconds(delay))
                            NextActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.Normal:
                        NextActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                }
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }
        }

        public bool IsReadyForNextAction()
        {
            _actionTimeLock.EnterReadLock();

            try
            {
                return Interval < LastActionStopwatch.Elapsed;
            }
            finally
            {
                _actionTimeLock.ExitReadLock();
            }
        }

        public void PauseAction()
        {
            _coroutineLock.EnterWriteLock();

            try
            {
                if(_actionCoroutine.IsRunning)
                    Timing.PauseCoroutines(_actionCoroutine);

                LastActionStopwatch.Stop();
            }
            finally
            {
                _coroutineLock.ExitWriteLock();
            }
        }

        public void ResumeAction()
        {
            _coroutineLock.EnterWriteLock();

            try
            {
                if(_actionCoroutine.IsAliveAndPaused)
                    Timing.ResumeCoroutines(_actionCoroutine);

                LastActionStopwatch.Start();
            }
            finally
            {
                _coroutineLock.ExitWriteLock();
            }
        }

        private IEnumerator<float> TaskCoroutineMethod()
        {
            while (true)
            {
                yield return Timing.WaitUntilTrue(() =>
                {
                    _actionTimeLock.EnterReadLock();

                    try
                    {
                        if (Interval > LastActionStopwatch.Elapsed)
                            return false;

                        if (DateTime.Now < NextActionTime)
                            return false;

                        return true;
                    }
                    finally
                    {
                        _actionTimeLock.ExitReadLock();
                    }
                });

                _actionTimeLock.EnterWriteLock();

                try
                {
                    LastActionStopwatch.Restart();
                    NextActionTime = DateTime.MaxValue;
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }

                try
                {
                    _action.Invoke();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }

        public enum DelayType
        {
            /// <summary>
            /// Only save the fastest action time
            /// </summary>
            Fastest,
            /// <summary>
            /// Only save the latest action time
            /// </summary>
            Latest,
            /// <summary>
            /// Update the action time without comparing
            /// </summary>
            Normal
        }
    }
}
