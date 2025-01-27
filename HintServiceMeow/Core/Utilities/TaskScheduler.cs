using HintServiceMeow.Core.Utilities.Tools;
using MEC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace HintServiceMeow.Core.Utilities
{
    internal class TaskScheduler
    {
        private readonly Action _action;

        private readonly TimeSpan Interval;

        public Stopwatch IntervalStopwatch { get; private set; } = Stopwatch.StartNew();

        public DateTime ScheduledActionTime { get; private set; } = new DateTime();

        private readonly ReaderWriterLockSlim _actionTimeLock = new ReaderWriterLockSlim();

        private bool Paused { get; set; } = false;

        public TaskScheduler(TimeSpan interval, Action action)
        {
            this.Interval = interval;
            this._action = action ?? throw new ArgumentNullException(nameof(action));

            if (interval > TimeSpan.Zero)
            {
                //Force change the elapsed time of the stopwatch
                //This is evil......
                _actionTimeLock.EnterWriteLock();
                try
                {
                    FieldInfo timerElapsedField = typeof(Stopwatch).GetField("elapsed", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
                    timerElapsedField.SetValue(IntervalStopwatch, interval.Ticks);
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }
            }

            MainThreadDispatcher.Dispatch(() => Timing.RunCoroutine(TaskCoroutineMethod()));
        }

        public void StartAction()
        {
            _actionTimeLock.EnterWriteLock();

            try
            {
                ScheduledActionTime = DateTime.MinValue;
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
                if (ScheduledActionTime == DateTime.MaxValue)
                {
                    ScheduledActionTime = DateTime.Now.AddSeconds(delay);
                    return;
                }

                switch (delayType)
                {
                    case DelayType.KeepFastest:
                        if (ScheduledActionTime > DateTime.Now.AddSeconds(delay))
                            ScheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.KeepLatest:
                        if (ScheduledActionTime < DateTime.Now.AddSeconds(delay))
                            ScheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.Override:
                        ScheduledActionTime = DateTime.Now.AddSeconds(delay);
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
                return Interval < IntervalStopwatch.Elapsed;
            }
            finally
            {
                _actionTimeLock.ExitReadLock();
            }
        }

        public void PauseIntervalStopwatch()
        {
            IntervalStopwatch.Stop();
            Paused = true;
        }

        public void ResumeIntervalStopwatch()
        {
            IntervalStopwatch.Start();
            Paused = false;
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
                        if (Interval > IntervalStopwatch.Elapsed)
                            return false;

                        if (DateTime.Now < ScheduledActionTime)
                            return false;

                        return !Paused;
                    }
                    catch (Exception ex)
                    {
                        LogTool.Error(ex);
                        return false;
                    }
                    finally
                    {
                        _actionTimeLock.ExitReadLock();
                    }
                });

                try
                {
                    _action.Invoke();
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }

                _actionTimeLock.EnterWriteLock();

                try
                {
                    IntervalStopwatch.Restart();
                    ScheduledActionTime = DateTime.MaxValue;
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }
            }
        }

        public enum DelayType
        {
            /// <summary>
            /// Only save the fastest action time
            /// </summary>
            KeepFastest,
            /// <summary>
            /// Only save the latest action time
            /// </summary>
            KeepLatest,
            /// <summary>
            /// Update the action time without comparing
            /// </summary>
            Override
        }
    }
}
