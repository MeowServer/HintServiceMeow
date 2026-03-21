namespace HintServiceMeow.Core.Utilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Utilities.Tools;

    internal class TaskScheduler : ITaskScheduler
    {
        private readonly ReaderWriterLockSlim schedulerLock = new();

        private readonly PeriodicRunner runner;

        private Func<bool> task;
        private DateTime scheduledActionTime; // Indicate when the timer will begin trying invoking the action
        private TimeSpan interval; // Minimum time between two actions
        private DateTime startTimeStamp; // Used to calculate elapsed time since last action, = DateTime.MinValue if there's no last action.
        private TimeSpan elapsed; // Time elapsed since last action, does not include the time when the scheduler is paused.
        private bool paused;

        public TaskScheduler(int tickRate = 30)
        {
            interval = TimeSpan.FromSeconds(0);
            task = () => true; // Default empty action
            startTimeStamp = DateTime.Now;

            runner = PeriodicRunner.Start(PeriodicRunnerMethod, TimeSpan.FromSeconds(1.0 / tickRate));
        }

        public bool InvokeUntilSuccess { get; set; }

        public bool IsPaused => paused;

        /// <summary>
        /// Gets the time elapsed since last action. Does not include the time when the scheduler is paused.
        /// If there's no last action, it is DateTime.Now - DateTime.MinValue.
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                schedulerLock.EnterWriteLock();
                try
                {
                    if (IsPaused)
                        return elapsed; // Do not calculate elapsed time during paused period

                    CalculateElapsedTime();
                    return elapsed;
                }
                finally
                {
                    schedulerLock.ExitWriteLock();
                }
            }

            private set
            {
                schedulerLock.EnterWriteLock();
                try
                {
                    elapsed = value; // Set elapsed time
                    startTimeStamp = DateTime.Now; // Reset time stamp
                }
                finally
                {
                    schedulerLock.ExitWriteLock();
                }
            }
        }

        public TimeSpan MinInterval
        {
            get
            {
                schedulerLock.EnterReadLock();
                try
                {
                    return interval;
                }
                finally
                {
                    schedulerLock.ExitReadLock();
                }
            }

            set
            {
                schedulerLock.EnterWriteLock();
                try
                {
                    if (value <= TimeSpan.Zero)
                        interval = TimeSpan.Zero;
                    else
                        interval = value;
                }
                finally
                {
                    schedulerLock.ExitWriteLock();
                }
            }
        }

        public bool IsReadyForNextAction => Elapsed >= interval;

        private DateTime ScheduledActionTime
        {
            get
            {
                schedulerLock.EnterReadLock();
                try
                {
                    return scheduledActionTime;
                }
                finally
                {
                    schedulerLock.ExitReadLock();
                }
            }

            set
            {
                schedulerLock.EnterWriteLock();
                try
                {
                    scheduledActionTime = value;
                }
                finally
                {
                    schedulerLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Not thread safe.
        /// <inheritdoc/>
        /// </summary>
        void IDisposable.Dispose()
        {
            runner.Dispose();
            schedulerLock.Dispose();
        }

        /// <summary>
        /// Start the scheduler with a specified interval and action.
        /// </summary>
        /// <param name="newInterval">Minimum interval between each action.</param>
        /// <param name="newAction">The action to invoke.</param>
        /// <exception cref="ArgumentNullException">newAction is null.</exception>
        public void Start(TimeSpan newInterval, Action newAction)
        {
            schedulerLock.EnterWriteLock();
            try
            {
                if (newInterval <= TimeSpan.Zero)
                    newInterval = TimeSpan.Zero;

                interval = newInterval;

                if (newAction is null)
                    throw new ArgumentNullException(nameof(newAction), "Action cannot be null.");

                task = () =>
                {
                    newAction();
                    return true; // Return true to indicate success
                };

                // Reset Elapsed
                elapsed = TimeSpan.Zero;
                startTimeStamp = DateTime.Now;

                // Reset scheduled action time
                scheduledActionTime = DateTime.MaxValue;
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Start the scheduler with a specified interval and action.
        /// </summary>
        /// <param name="newInterval">Minimum interval between each action.</param>
        /// <param name="newAction">The action to invoke.</param>
        /// <exception cref="ArgumentNullException">newAction is null.</exception>
        public void Start(TimeSpan newInterval, Func<bool> newAction)
        {
            schedulerLock.EnterWriteLock();
            try
            {
                if (newInterval <= TimeSpan.Zero)
                    newInterval = TimeSpan.Zero;

                // Set new interval and action
                interval = newInterval;
                task = newAction ?? throw new ArgumentNullException(nameof(newAction), "Action cannot be null.");

                // Reset Elapsed
                elapsed = TimeSpan.Zero;
                startTimeStamp = DateTime.Now;

                // Reset scheduled action time
                scheduledActionTime = DateTime.MaxValue;
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Schedule an action to be invoked after a specified delay. The action will be invoked when the elapsed time reaches the interval limit and when scheduled action time is reached.
        /// </summary>
        /// <param name="delay">How long scheduler should wait before trying to invoke the action.</param>
        /// <param name="delayType">What to do if there's already a scheduled action.</param>
        public void Invoke(float delay = -1f, DelayType delayType = DelayType.Override)
        {
            schedulerLock.EnterWriteLock();

            try
            {
                // If there's not scheduled time, then set it to the current time plus delay
                if (scheduledActionTime == DateTime.MaxValue)
                {
                    scheduledActionTime = DateTime.Now.AddSeconds(delay);
                    return;
                }

                // If there is a scheduled time, set based on the DelayType passed in
                switch (delayType)
                {
                    case DelayType.KeepFastest:
                        if (scheduledActionTime > DateTime.Now.AddSeconds(delay))
                            scheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.KeepSlowest:
                        if (scheduledActionTime < DateTime.Now.AddSeconds(delay))
                            scheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    case DelayType.Override:
                        scheduledActionTime = DateTime.Now.AddSeconds(delay);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(delayType), delayType, null);
                }
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Stop the scheduler and reset the action and interval.
        /// </summary>
        public void Stop()
        {
            schedulerLock.EnterWriteLock();
            try
            {
                // Reset the action and interval
                task = () => true; // Default empty action
                interval = TimeSpan.FromSeconds(0);
                scheduledActionTime = DateTime.MaxValue; // Reset scheduled action time

                // Reset Elapsed
                elapsed = TimeSpan.Zero;
                startTimeStamp = DateTime.Now;
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }

        public void Pause()
        {
            schedulerLock.EnterWriteLock();
            try
            {
                if (paused)
                    return;

                CalculateElapsedTime(); // Add time to the timer before pausing

                paused = true;
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }

        public void Resume()
        {
            schedulerLock.EnterWriteLock();
            try
            {
                if (!paused)
                    return;

                paused = false;
                startTimeStamp = DateTime.Now; // Reset time stamp
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Invoke the action and reset the timer and scheduled action time.
        /// </summary>
        private void InvokeAction()
        {
            try
            {
                // start action
                if (task.Invoke() || !InvokeUntilSuccess)
                {
                    // Reset Timer
                    schedulerLock.EnterWriteLock();
                    try
                    {
                        // Reset Elapsed
                        elapsed = TimeSpan.Zero;
                        startTimeStamp = DateTime.Now;

                        // Reset timer
                        scheduledActionTime = DateTime.MaxValue;
                    }
                    finally
                    {
                        schedulerLock.ExitWriteLock();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);

                if (!InvokeUntilSuccess)
                {
                    // Reset Timer
                    schedulerLock.EnterWriteLock();
                    try
                    {
                        // Reset Elapsed
                        elapsed = TimeSpan.Zero;
                        startTimeStamp = DateTime.Now;

                        // Reset timer
                        scheduledActionTime = DateTime.MaxValue;
                    }
                    finally
                    {
                        schedulerLock.ExitWriteLock();
                    }
                }
            }
        }

        private void CalculateElapsedTime()
        {
            // If the scheduled action time is in the future, skip
            if (startTimeStamp > DateTime.Now)
                return;

            elapsed += DateTime.Now - startTimeStamp; // Calculate elapsed time
            startTimeStamp = DateTime.Now; // Reset time stamp
        }

        private Task PeriodicRunnerMethod()
        {
            // Check if the action should be executed, if not, continue, else, break the loop
            try
            {
                if (!IsReadyForNextAction || ScheduledActionTime == DateTime.MaxValue || ScheduledActionTime > DateTime.Now || IsPaused)
                    return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }

            InvokeAction();

            return Task.CompletedTask;
        }
    }
}
