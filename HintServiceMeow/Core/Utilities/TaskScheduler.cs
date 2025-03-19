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

        private readonly TimeSpan _interval;

        public bool Paused { get; set; }

        public Stopwatch IntervalStopwatch { get; } = Stopwatch.StartNew();
        public DateTime ScheduledActionTime { get; private set; }
        
        public CoroutineHandle Coroutine { get; set; }

        private readonly ReaderWriterLockSlim _actionTimeLock = new();

        public TaskScheduler(TimeSpan interval, Action action)
        {
            this._interval = interval;
            this._action = action ?? throw new ArgumentNullException(nameof(action));

            if (interval > TimeSpan.Zero)
            {
                //Force to change the elapsed time of the stopwatch
                //This is evil......
                _actionTimeLock.EnterWriteLock();
                try
                {
                    FieldInfo timerElapsedField = typeof(Stopwatch).GetField("elapsed", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
                    timerElapsedField?.SetValue(IntervalStopwatch, interval.Ticks);
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }
            }

            MainThreadDispatcher.Dispatch(() => Coroutine = Timing.RunCoroutine(TaskCoroutineMethod()));
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
                return _interval < IntervalStopwatch.Elapsed;
            }
            finally
            {
                _actionTimeLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Not thread safe
        /// </summary>
        public void Destruct()
        {
            Timing.KillCoroutines(this.Coroutine);
        }
        
        private IEnumerator<float> TaskCoroutineMethod()
        {
            while (true)
            {
                while (true)
                {
                    yield return Timing.WaitForOneFrame;

                    _actionTimeLock.EnterReadLock();

                    //Reset error flag
                    bool isSuccessful = true;

                    try
                    {
                        //Check if the action should be executed, if not, continue, else, break the loop
                        if (_interval > IntervalStopwatch.Elapsed)
                            continue;

                        if (DateTime.Now < ScheduledActionTime)
                            continue;

                        if (!Paused)
                            break;
                    }
                    catch (Exception ex)
                    {
                        LogTool.Error(ex);
                        isSuccessful = false; //If an error occurs, set error flag to false
                    }
                    finally
                    {
                        _actionTimeLock.ExitReadLock();
                    }

                    //If an error occurs, wait for a while so it will not stuck the log.
                    if (!isSuccessful)
                    {
                        yield return Timing.WaitForSeconds(0.5f);
                    }
                }

                //Reset timer
                _actionTimeLock.EnterWriteLock();
                try
                {
                    IntervalStopwatch.Restart();//Reset timer
                    ScheduledActionTime = DateTime.MaxValue; //Reset scheduled action time
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }

                //start action
                try
                {
                    _action.Invoke();
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
            }
        }

        public enum DelayType
        {
            /// <summary>
            /// Only keep the fastest action time
            /// </summary>
            KeepFastest,
            /// <summary>
            /// Only keep the latest action time
            /// </summary>
            KeepLatest,
            /// <summary>
            /// Update the action time without comparing
            /// </summary>
            Override
        }
    }
}
