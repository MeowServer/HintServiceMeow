using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Utilities.Tools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    internal class TaskScheduler : Interface.ITaskScheduler, Interface.IDestructible
    {
        private readonly ReaderWriterLockSlim _actionTimeLock = new();

        private Action _action;
        private DateTime _scheduledActionTime; // Indicate when the timer will begin trying invoking the action
        private TimeSpan _interval; // Minimum time between two actions
        private DateTime _startTimeStamp; // Used to calculate elapsed time since last action, = DateTime.MinValue if there's no last action.
        private TimeSpan _elapsed; // Time elapsed since last action, does not include the time when the scheduler is paused.
        private bool _paused = false;
        private readonly PeriodicRunner _runner;
        public bool IsPaused => _paused;

        /// <summary>
        /// Time elapsed since last action. Does not include the time when the scheduler is paused.
        /// If there's no last action, it is DateTime.Now - DateTime.MinValue.
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                _actionTimeLock.EnterWriteLock();
                try
                {
                    if (IsPaused)
                        return _elapsed; // Do not calculate elapsed time during paused period

                    CalculateElapsedTime();
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

        public TaskScheduler(int tickRate = 30)
        {
            this._interval = TimeSpan.FromSeconds(0);
            this._action = () => { }; // Default empty action
            _startTimeStamp = DateTime.MinValue; // Default zero interval

            _runner = PeriodicRunner.Start(PeriodicRunnerMethod, TimeSpan.FromSeconds(1.0 / tickRate));
        }

        /// <summary>
        /// Not thread safe
        /// </summary>
        void Interface.IDestructible.Destruct()
        {
            _runner.Dispose();
        }

        /// <summary>
        /// Start the scheduler with a specified interval and action.
        /// </summary>
        /// <param name="interval">Minimum interval between each action</param>
        /// <param name="action">The action to invoke</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void Start(TimeSpan interval, Action action)
        {
            _actionTimeLock.EnterWriteLock();
            try
            {
                if (interval <= TimeSpan.Zero)
                    interval = TimeSpan.Zero;
                _interval = interval;
                _action = action ?? throw new ArgumentNullException(nameof(action), "Action cannot be null.");
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }

            // Reset the timer
            Elapsed = TimeSpan.Zero;
            ScheduledActionTime = DateTime.MaxValue; // Reset scheduled action time
        }

        /// <summary>
        /// Schedule an action to be invoked after a specified delay. The action will be invoked when the elapsed time reaches the interval limit and when scheduled action time is reached.
        /// </summary>
        /// <param name="delay">How long scheduler should wait before trying to invoek the action</param>
        /// <param name="delayType">What to do if there's already a scheduled action</param>
        public void Invoke(float delay = -1f, DelayType delayType = DelayType.Override)
        {
            _actionTimeLock.EnterWriteLock();

            try
            {
                // If there's not scheduled time, then set it to the current time plus delay
                if (_scheduledActionTime == DateTime.MaxValue)
                {
                    _scheduledActionTime = DateTime.Now.AddSeconds(delay);
                    return;
                }

                // If there is a scheduled time, set based on the DelayType passed in
                switch (delayType)
                {
                    case DelayType.KeepFastest:
                        if (_scheduledActionTime > DateTime.Now.AddSeconds(delay))
                            _scheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.KeepSlowest:
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
        /// Stop the scheduler and reset the action and interval.
        /// </summary>
        public void Stop()
        {
            _actionTimeLock.EnterWriteLock();
            try
            {
                // Reset the action and interval
                _action = () => { }; // Default empty action
                _interval = TimeSpan.FromSeconds(0);
                _scheduledActionTime = DateTime.MaxValue; // Reset scheduled action time
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }

            Elapsed = TimeSpan.Zero; // Reset elapsed time
            ScheduledActionTime = DateTime.MaxValue; // Reset scheduled action time
        }

        public void Pause()
        {
            _actionTimeLock.EnterWriteLock();
            try
            {
                if (_paused)
                    return;

                CalculateElapsedTime(); // Add time to the timer before pausing

                _paused = true;
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }
        }

        public void Resume()
        {
            _actionTimeLock.EnterWriteLock();
            try
            {
                if (!_paused)
                    return;

                _paused = false;
                _startTimeStamp = DateTime.Now; // Reset time stamp
            }
            finally
            {
                _actionTimeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Invoke the action and reset the timer and scheduled action time.
        /// </summary>
        private void InvokeAction()
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
                Logger.Instance.Error(ex);
            }
        }

        private void CalculateElapsedTime()
        {
            // If the scheduled action time is in the future, skip
            if (_startTimeStamp > DateTime.Now)
                return;

            _elapsed += DateTime.Now - _startTimeStamp; // Calculate elapsed time
            _startTimeStamp = DateTime.Now; //Reset time stamp
        }

        private async Task PeriodicRunnerMethod()
        {
            //Check if the action should be executed, if not, continue, else, break the loop
            try
            {
                if (!IsReadyForNextAction || ScheduledActionTime == DateTime.MaxValue || DateTime.Now < ScheduledActionTime || IsPaused)
                    return;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }

            InvokeAction();
        }
    }
}
