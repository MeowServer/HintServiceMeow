namespace HintServiceMeow.Core.Utilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HintServiceMeow.Core.Utilities.Tools;

    /// <summary>
    /// Executes an asynchronous action on a recurring interval, with support for pausing and disposal.
    /// </summary>
    internal class PeriodicRunner : IDisposable
    {
        private readonly CancellationTokenSource cts = new();
        private readonly TimeSpan interval;
        private readonly Func<Task> actionAsync;
        private readonly Task loopTask;
        private readonly object pauseLock = new();

        private bool paused;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicRunner"/> class.
        /// Create a runner that executes an action periodically.
        /// </summary>
        /// <param name="actionAsync">The action that runs periodically.</param>
        /// <param name="interval">The minimum interval between each action.</param>
        /// <param name="runImmediately">Whether to run immediately after this call.</param>
        private PeriodicRunner(Func<Task> actionAsync, TimeSpan interval, bool runImmediately = false)
        {
            this.actionAsync = actionAsync ?? throw new ArgumentNullException(nameof(actionAsync));
            this.interval = interval >= TimeSpan.Zero ? interval : throw new ArgumentOutOfRangeException(nameof(interval));
            loopTask = RunLoopAsync(runImmediately, cts.Token);
        }

        /// <summary>
        /// Gets the current task that runs the periodic action.
        /// </summary>
        public Task CurrentTask => loopTask;

        /// <summary>
        /// Creates and starts a new <see cref="PeriodicRunner"/> that repeatedly invokes the specified action.
        /// </summary>
        /// <param name="actionAsync">The async action to invoke periodically.</param>
        /// <param name="interval">The minimum interval between invocations.</param>
        /// <param name="runImmediately">Whether to run the action immediately on start.</param>
        /// <returns>A running <see cref="PeriodicRunner"/> instance.</returns>
        public static PeriodicRunner Start(
            Func<Task> actionAsync,
            TimeSpan interval,
            bool runImmediately = false)
            => new(actionAsync, interval, runImmediately);

        /// <summary>
        /// Pauses periodic execution until <see cref="Resume"/> is called.
        /// </summary>
        public void Pause()
        {
            lock (pauseLock)
            {
                paused = true;
            }
        }

        /// <summary>
        /// Resumes periodic execution after a call to <see cref="Pause"/>.
        /// </summary>
        public void Resume()
        {
            lock (pauseLock)
            {
                paused = false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }

        private async Task RunLoopAsync(bool runImmediately, CancellationToken token)
        {
            try
            {
                if (runImmediately)
                    await InvokeActionSafeAsync(token).ConfigureAwait(false);

                DateTime nextDue = DateTime.UtcNow + interval;

                while (!token.IsCancellationRequested)
                {
                    TimeSpan delay = nextDue - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, token).ConfigureAwait(false);

                    if (!IsPaused())
                    {
                        await InvokeActionSafeAsync(token).ConfigureAwait(false);
                        nextDue = DateTime.UtcNow + interval;
                    }
                    else
                    {
                        // Paused
                        await Task.Delay(interval, token).ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            } // Action cancelled
        }

        private bool IsPaused()
        {
            lock (pauseLock)
                return paused;
        }

        private async Task InvokeActionSafeAsync(CancellationToken token)
        {
            try
            {
                await actionAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Cancellation requested, do nothing
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in periodic action: {ex.Message}");
            }
        }
    }
}
