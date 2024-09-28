using System;
using System.Collections.Generic;
using System.Threading;

using PluginAPI.Core;
using MEC;
using static HintServiceMeow.Core.Utilities.TaskScheduler;

namespace HintServiceMeow.Core.Utilities
{
    internal class TaskScheduler
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly Action _action;

        public readonly TimeSpan Interval;

        public DateTime LastActionTime { get; private set; } = DateTime.MinValue;
        public DateTime NextActionTime { get; set; } = DateTime.MaxValue;

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

        public TaskScheduler(TimeSpan interval, Action action)
        {
            this.Interval = interval;
            this._action = action;
            Timing.RunCoroutine(RunTasks());
        }

        public void StartAction()
        {
            _lock.EnterWriteLock();

            try
            {
                NextActionTime = DateTime.MinValue;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void StartAction(float delay, DelayType delayType)
        {
            _lock.EnterWriteLock();

            try
            {
                switch (delayType)
                {
                    case DelayType.Fastest:
                        if (NextActionTime - DateTime.Now > TimeSpan.FromSeconds(delay))
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
                _lock.ExitWriteLock();
            }
        }

        public void ResetCountDown()
        {
            _lock.EnterWriteLock();

            try
            {
                LastActionTime = DateTime.Now;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool IsReadyForNextAction()
        {
            _lock.EnterReadLock();

            try
            {
                return DateTime.Now > LastActionTime + Interval;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private IEnumerator<float> RunTasks()
        {
            while (true)
            {
                yield return Timing.WaitUntilTrue(() =>
                {
                    _lock.EnterReadLock();

                    try
                    {
                        if (DateTime.Now < LastActionTime + Interval)
                            return false;

                        if (DateTime.Now < NextActionTime)
                            return false;

                        return true;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                });

                try
                {
                    _lock.EnterWriteLock();

                    try
                    {
                        LastActionTime = DateTime.Now;

                        _action.Invoke();

                        NextActionTime = DateTime.MaxValue;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
    }
}
