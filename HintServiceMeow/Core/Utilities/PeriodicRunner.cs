using HintServiceMeow.Core.Utilities.Tools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    internal class PeriodicRunner : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly TimeSpan _interval;
        private readonly Func<Task> _actionAsync;
        private readonly Task _loopTask;
        private readonly object _pauseLock = new object();

        private bool _paused;

        /// <summary>
        /// Create a runner that executes an action periodically.
        /// </summary>
        /// <param name="actionAsync">Action runs periodically</param>
        /// <param name="interval">Minimum interval between each action</param>
        /// <param name="runImmediately">Whether to run immediately after this call</param>
        private PeriodicRunner(Func<Task> actionAsync, TimeSpan interval, bool runImmediately = false)
        {
            _actionAsync = actionAsync ?? throw new ArgumentNullException(nameof(actionAsync));
            _interval = interval >= TimeSpan.Zero
                         ? interval
                         : throw new ArgumentOutOfRangeException(nameof(interval));
            _loopTask = RunLoopAsync(runImmediately, _cts.Token);
        }

        public static PeriodicRunner Start(
            Func<Task> actionAsync,
            TimeSpan interval,
            bool runImmediately = false)
            => new PeriodicRunner(actionAsync, interval, runImmediately);

        public void Pause()
        {
            lock (_pauseLock)
            {
                _paused = true;
            }
        }

        public void Resume()
        {
            lock (_pauseLock)
            {
                _paused = false;
            }
        }
        /// <summary>
        /// Current task that runs the periodic action.
        /// </summary>
        public Task CurrentTask => _loopTask;

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private async Task RunLoopAsync(bool runImmediately, CancellationToken token)
        {
            try
            {
                if (runImmediately)
                    await InvokeActionSafeAsync(token).ConfigureAwait(false);

                var nextDue = DateTime.UtcNow + _interval;

                while (!token.IsCancellationRequested)
                {
                    var delay = nextDue - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, token).ConfigureAwait(false);

                    if (!IsPaused())
                    {
                        await InvokeActionSafeAsync(token).ConfigureAwait(false);
                        nextDue = DateTime.UtcNow + _interval;
                    }
                    else
                    {
                        // Paused
                        await Task.Delay(_interval, token).ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException) { } // Action cancelled
        }

        private bool IsPaused()
        {
            lock (_pauseLock) return _paused;
        }

        private async Task InvokeActionSafeAsync(CancellationToken token)
        {
            try
            {
                await _actionAsync().ConfigureAwait(false);
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
