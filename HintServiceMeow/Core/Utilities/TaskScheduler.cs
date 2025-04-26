using HintServiceMeow.Core.Utilities.Tools;
using MEC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace HintServiceMeow.Core.Utilities
{
    internal class TaskScheduler : Interface.IDestructible
    {
        private readonly ReaderWriterLockSlim _actionTimeLock = new();

        private readonly Action _action;
        private DateTime _scheduledActionTime; // Indicate when the timer will begin trying invoking the action
        private readonly TimeSpan _interval; // Minimum time between two actions
        private DateTime _startTimeStamp;
        private TimeSpan _elapsed;
        private bool _paused = false;
        private CoroutineHandle _coroutine;

        public bool Paused
        {
            get => _paused;
            set
            {
                _actionTimeLock.EnterWriteLock();
                try
                {
                    if(value)
                    {
                        _elapsed += DateTime.Now - _startTimeStamp; //Stop Timer
                    }
                    else
                    {
                        _startTimeStamp = DateTime.Now; //Start new timer
                    }

                    _paused = value;
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Time elapsed since last action. Does not include the time when the scheduler is paused.
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                _actionTimeLock.EnterWriteLock();
                try
                {
                    if (Paused)
                        return _elapsed;

                    _elapsed += DateTime.Now - _startTimeStamp;
                    _startTimeStamp = DateTime.Now;
                    return _elapsed;
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }
            }
            private set
            {
                _actionTimeLock.EnterWriteLock();
                try
                {
                    _elapsed = value; // Set elapsed time
                    _startTimeStamp = DateTime.Now; // Reset time stamp
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }
            }
        }

        private DateTime ScheduledActionTime
        {
            get
            {
                _actionTimeLock.EnterReadLock();
                try
                {
                    return _scheduledActionTime;
                }
                finally
                {
                    _actionTimeLock.ExitReadLock();
                }
            }
            set
            {
                _actionTimeLock.EnterWriteLock();
                try
                {
                    _scheduledActionTime = value;
                }
                finally
                {
                    _actionTimeLock.ExitWriteLock();
                }
            }
        }

        public bool IsReadyForNextAction => Elapsed >= _interval;

        public TaskScheduler(TimeSpan interval, Action action)
        {
            this._interval = interval;
            this._action = action ?? throw new ArgumentNullException(nameof(action));
            _startTimeStamp = DateTime.MinValue;

            MainThreadDispatcher.Dispatch(() => _coroutine = Timing.RunCoroutine(TaskCoroutineMethod()));
        }

        /// <summary>
        /// Not thread safe
        /// </summary>
        void Interface.IDestructible.Destruct()
        {
            Timing.KillCoroutines(this._coroutine);
        }

        public void ScheduleAction(float delay = -1f, DelayType delayType = DelayType.Override)
        {
            _actionTimeLock.EnterWriteLock();

            try
            {
                if (_scheduledActionTime == DateTime.MaxValue)
                {
                    _scheduledActionTime = DateTime.Now.AddSeconds(delay);
                    return;
                }

                switch (delayType)
                {
                    case DelayType.KeepFastest:
                        if (_scheduledActionTime > DateTime.Now.AddSeconds(delay))
                            _scheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.KeepLatest:
                        if (_scheduledActionTime < DateTime.Now.AddSeconds(delay))
                            _scheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.Override:
                        _scheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                }
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Invoke the action and reset the timer and scheduled action time.
        /// </summary>
        internal void InvokeAction()
        {
            try
            {
                //Reset timer
                Elapsed = TimeSpan.Zero; //Reset Elapsed Time
                ScheduledActionTime = DateTime.MaxValue; //Reset scheduled action time

                //start action
                _action.Invoke();
            }
            catch (Exception ex)
            {
                LogTool.Error(ex);
            }
        }

        private IEnumerator<float> TaskCoroutineMethod()
        {
            while (true)
            {
                while (true)
                {
                    yield return Timing.WaitForOneFrame;

                    //Reset error flag
                    bool isSuccessful = true;

                    //Check if the action should be executed, if not, continue, else, break the loop
                    try
                    {
                        if (_interval > Elapsed || ScheduledActionTime == DateTime.MaxValue || DateTime.Now < ScheduledActionTime || Paused)
                            continue;

                        break;
                    }
                    catch (Exception ex)
                    {
                        LogTool.Error(ex);
                        isSuccessful = false; //If an error occurs, set error flag to false
                    }

                    //If an error occurs, wait for a while so it will not stuck the log.
                    if (!isSuccessful)
                    {
                        yield return Timing.WaitForSeconds(0.5f);
                    }
                }

                InvokeAction();
            }
        }

        public enum DelayType
        {
            /// <summary>
            /// Only keep the fastest scheduled action time
            /// </summary>
            KeepFastest,
            /// <summary>
            /// Only keep the latest scheduled action time
            /// </summary>
            KeepLatest,
            /// <summary>
            /// Update the action time without comparing
            /// </summary>
            Override
        }
    }
}
