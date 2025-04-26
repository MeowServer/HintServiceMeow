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
        private readonly TimeSpan _interval;
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
                    _actionTimeLock.EnterWriteLock();
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
                    _actionTimeLock.EnterWriteLock();
                }
            }
        }

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

        public void StartAction()
        {
            _actionTimeLock.EnterWriteLock();

            try
            {
                _scheduledActionTime = DateTime.MinValue;
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

        public bool IsReadyForNextAction()
        {
            _actionTimeLock.EnterReadLock();

            try
            {
                return _interval < Elapsed;
            }
            finally
            {
                _actionTimeLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Invoke the action and reset the timer and scheduled action time.
        /// </summary>
        internal void InvokeAction()
        {
            //Reset timer
            _actionTimeLock.EnterWriteLock();
            try
            {
                Elapsed = TimeSpan.Zero; //Reset Elapsed Time
                _scheduledActionTime = DateTime.MaxValue; //Reset scheduled action time
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

                    //Check if the action should be executed, if not, continue, else, break the loop
                    try
                    {
                        if (_interval > Elapsed) // If the interval is not reached, continue
                            continue;

                        if (_scheduledActionTime == DateTime.MaxValue || DateTime.Now < _scheduledActionTime) // If the action is not scheduled or the scheduled time is not reached, continue
                            continue;

                        if (Paused) // If the action is paused, continue
                            continue;

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
