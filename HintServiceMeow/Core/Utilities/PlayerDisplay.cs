using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Tools;
using MEC;
using Mirror;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Represent a player's display. This class is used to manage hints and update hint to player's display
    /// </summary>
    public class PlayerDisplay
    {
        /// <summary>
        /// The player this instance bind to
        /// </summary>
        public ReferenceHub ReferenceHub { get; }
        public NetworkConnection ConnectionToClient { get; }

        /// <summary>
        /// Invoke every tick when ReferenceHub display is ready to update.
        /// </summary>
        public event UpdateAvailableEventHandler UpdateAvailable;
        public delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev);

        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = new HashSet<PlayerDisplay>();
        private static readonly object _playerDisplayListLock = new();

        private readonly ConcurrentBag<IDisplayOutput> _displayOutputs = new ConcurrentBag<IDisplayOutput> { new DefaultDisplayOutput() };

        private readonly HintCollection _hints = new();
        private readonly TaskScheduler _taskScheduler;//Initialize in constructor
        private IHintParser _hintParser = new HintParser();
        private ICompatibilityAdaptor _adapter;//Initialize in constructor

        private CoroutineHandle _coroutine;//Initialize in constructor(MultiThreadTool)

        private Task _currentParserTask;
        private readonly object _currentParserTaskLock = new();

        private PlayerDisplay(ReferenceHub referenceHub)
        {
            this.ConnectionToClient = referenceHub.netIdentity.connectionToClient;
            this.ReferenceHub = referenceHub;

            this._taskScheduler = new TaskScheduler(TimeSpan.FromSeconds(0.5f), () =>
            {
                StartParserTask();
                _taskScheduler?.PauseIntervalStopwatch();//Pause action until the parser task is finishing
            });

            this._adapter = new CompatibilityAdaptor(this);
            MainThreadDispatcher.Dispatch(() => this._coroutine = Timing.RunCoroutine(CoroutineMethod()));
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            lock (_playerDisplayListLock)
            {
                PlayerDisplay pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

                if (pd is null)
                    return;

                Timing.KillCoroutines(pd._coroutine);
                PlayerDisplayList.Remove(pd);
            }
        }

        internal static void ClearInstance()
        {
            lock (_playerDisplayListLock)
            {
                PlayerDisplayList.Clear();
            }
        }

        public ConcurrentBag<IDisplayOutput> DisplayOutputs => _displayOutputs;

        public IHintParser HintParser
        {
            get => _hintParser;
            set => _hintParser = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ICompatibilityAdaptor CompatibilityAdaptor
        {
            get => _adapter;
            set => _adapter = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerator<float> CoroutineMethod()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;

                try
                {
                    //Periodic update
                    if (_taskScheduler.IntervalStopwatch.Elapsed > TimeSpan.FromSeconds(5))
                        ScheduleUpdate();

                    if (_taskScheduler.IsReadyForNextAction())
                    {
                        UpdateAvailable?.Invoke(new UpdateAvailableEventArg(this));
                    }
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
            }
        }

        private void OnHintUpdate(AbstractHint hint)
        {
            if (hint.SyncSpeed == HintSyncSpeed.UnSync)
                return;

            float maxWaitingTime = hint.SyncSpeed switch
            {
                HintSyncSpeed.Fastest => 0,
                HintSyncSpeed.Fast => 0.1f,
                HintSyncSpeed.Normal => 0.3f,
                HintSyncSpeed.Slow => 1f,
                HintSyncSpeed.Slowest => 3f,
                _ => throw new ArgumentOutOfRangeException()
            };

            ScheduleUpdate(maxWaitingTime, hint);
        }

        private void ScheduleUpdate(float maxWaitingTime = float.MinValue, AbstractHint updatingHint = null)
        {
            lock (_currentParserTaskLock)
            {
                if (_currentParserTask is not null)
                    return;
            }

            if (maxWaitingTime <= 0)
            {
                _taskScheduler.StartAction();
                return;
            }

            IEnumerable<AbstractHint> predictingHints = _hints.AllGroups.SelectMany(x => x);

            if (updatingHint != null)
            {
                predictingHints = predictingHints.Where(h => h.SyncSpeed >= updatingHint.SyncSpeed && h != updatingHint);
            }

            TimeSpan maxWaitingTimeSpan = TimeSpan.FromSeconds(maxWaitingTime);
            DateTime now = DateTime.Now;

            DateTime predictedUpdatingTime = predictingHints
                .Select(h => h.UpdateAnalyser.EstimateNextUpdate())
                .Where(x => x - now >= TimeSpan.Zero && x - now <= maxWaitingTimeSpan)
                .DefaultIfEmpty(now)
                .Max();

            float delay = (float)(predictedUpdatingTime - now).TotalSeconds;

            if (delay <= 0)
                _taskScheduler.StartAction();
            else
                _taskScheduler.StartAction(delay, TaskScheduler.DelayType.KeepFastest);
        }

        /// <summary>
        /// Force an update when the update is available. You do not have to use this method unless you are using UnSync hints
        /// </summary>
        public void ForceUpdate(bool useFastUpdate = false)
        {
            ScheduleUpdate(useFastUpdate ? 0f : 0.3f);
        }

        private void StartParserTask()
        {
            lock (_currentParserTaskLock)
            {
                if (_currentParserTask is not null)
                    return;

                _currentParserTask =
                    Task.Run<string>(() =>
                    {
                        try
                        {
                            return _hintParser.ParseToMessage(_hints);
                        }
                        catch (Exception ex)
                        {
                            LogTool.Error(ex);
                            return string.Empty;
                        }
                    })
                    .ContinueWith(parserTask =>
                    {
                        MainThreadDispatcher.Dispatch(() =>
                        {
                            try
                            {
                                SendHint(parserTask.Result);
                            }
                            catch (Exception ex)
                            {
                                LogTool.Error(ex);
                            }
                            finally
                            {
                                _taskScheduler.ResumeIntervalStopwatch();

                                lock (_currentParserTaskLock)
                                {
                                    _currentParserTask = null;
                                }
                            }
                        });
                    });
            }
        }

        private void SendHint(string text)
        {
            foreach (IDisplayOutput output in _displayOutputs.ToArray())
            {
                try
                {
                    output.ShowHint(new DisplayOutputArg(this, text));
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
            }
        }

        /// <summary>
        /// Get the PlayerDisplay instance of the player. If the instance have not been created yet, then it will create one.
        /// Not Thread Safe
        /// </summary>
        public static PlayerDisplay Get(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new ArgumentNullException(nameof(referenceHub));

            lock (_playerDisplayListLock)
            {
                PlayerDisplay existing = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

                if (existing is not null)
                    return existing;

                PlayerDisplay newPlayerDisplay = new PlayerDisplay(referenceHub);
                PlayerDisplayList.Add(newPlayerDisplay);
                return newPlayerDisplay;
            }
        }

        /// <summary>
        /// Get the PlayerDisplay instance of the player. If the instance have not been created yet, then it will create one.
        /// Not Thread Safe
        /// </summary>
        public static PlayerDisplay Get(PluginAPI.Core.Player player)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }

#if EXILED
        /// <summary>
        /// Get the PlayerDisplay instance of the player. If the instance have not been created yet, then it will create one.
        /// Not Thread Safe
        /// </summary>
        public static PlayerDisplay Get(Exiled.API.Features.Player player)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }
#endif

        public void AddHint(AbstractHint hint)
        {
            if (hint is null)
                return;

            this.InternalAddHint(Assembly.GetCallingAssembly().FullName, hint);
        }

        public void AddHint(IEnumerable<AbstractHint> hints)
        {
            if (hints is null)
                return;

            this.InternalAddHint(Assembly.GetCallingAssembly().FullName, hints);
        }

        public void RemoveHint(AbstractHint hint)
        {
            if (hint is null)
                return;

            this.InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);
        }

        public void RemoveHint(IEnumerable<AbstractHint> hints)
        {
            if (hints is null)
                return;

            this.InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hints);
        }

        public void RemoveHint(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to RemoveHint");

            this.InternalRemoveHint(Assembly.GetCallingAssembly().FullName, id);
        }

        public void RemoveHint(Guid id)
        {
            this.InternalRemoveHint(Assembly.GetCallingAssembly().FullName, id);
        }

        public void ClearHint()
        {
            this.InternalClearHint(Assembly.GetCallingAssembly().FullName);
        }

        public AbstractHint GetHint(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to GetHint");

            return _hints.GetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id).FirstOrDefault();
        }

        public AbstractHint GetHint(Guid id)
        {
            return _hints.GetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == id).FirstOrDefault();
        }

        internal void InternalAddHint(string name, AbstractHint hint, bool update = true)
        {
            hint.HintUpdated += OnHintUpdate;
            UpdateAvailable += hint.TryUpdateHint;

            _hints.AddHint(name, hint);

            if (update)
                ScheduleUpdate();
        }

        internal void InternalAddHint(string name, IEnumerable<AbstractHint> hints, bool update = true)
        {
            foreach (AbstractHint hint in hints)
            {
                hint.HintUpdated += OnHintUpdate;
                UpdateAvailable += hint.TryUpdateHint;

                _hints.AddHint(name, hint);
            }

            if (update)
                ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, AbstractHint hint, bool update = true)
        {
            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, hint);

            if (update)
                ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, IEnumerable<AbstractHint> hints, bool update = true)
        {
            foreach (AbstractHint hint in hints)
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;

                _hints.RemoveHint(name, hint);
            }

            if (update)
                ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, Guid id, bool update = true)
        {
            AbstractHint hint = _hints.GetHints(name).FirstOrDefault(x => x.Id.Equals(id));

            if (hint == null)
                return;

            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, x => x.Guid.Equals(id));

            if (update)
                ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, string id, bool update = true)
        {
            AbstractHint hint = _hints.GetHints(name).FirstOrDefault(x => x.Id.Equals(id));

            if (hint == null)
                return;

            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, x => x.Id.Equals(id));

            if (update)
                ScheduleUpdate();
        }

        internal void InternalClearHint(string name, bool update = true)
        {
            foreach (AbstractHint hint in _hints.GetHints(name).ToList())
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            _hints.ClearHints(name);

            if (update)
                ScheduleUpdate();
        }

        internal void ShowCompatibilityHint(string assemblyName, string content, float duration) => this._adapter.ShowHint(new Models.Arguments.CompatibilityAdaptorArg(assemblyName, content, duration));

        /// <summary>
        /// Argument for UpdateAvailable Event
        /// </summary>
        public class UpdateAvailableEventArg
        {
            public PlayerDisplay PlayerDisplay { get; set; }

            internal UpdateAvailableEventArg(PlayerDisplay playerDisplay)
            {
                this.PlayerDisplay = playerDisplay;
            }
        }
    }
}
