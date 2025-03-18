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
using System.ComponentModel;
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
        private static readonly object _playerDisplayListLock = new object();

        private readonly ConcurrentBag<IDisplayOutput> _displayOutputs = new ConcurrentBag<IDisplayOutput> { new DefaultDisplayOutput() };

        private readonly HintCollection _hints = new HintCollection();
        private readonly TaskScheduler _taskScheduler;//Initialize in constructor
        private IHintParser _hintParser = new HintParser();
        private ICompatibilityAdaptor _adapter;//Initialize in constructor

        private CoroutineHandle _coroutine;//Initialize in constructor(MultiThreadTool)

        private Task _currentParserTask;
        private readonly object _currentParserTaskLock = new object();

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

            this._hints.CollectionChanged += (_, __) => ScheduleUpdate();

            MainThreadDispatcher.Dispatch(() => this._coroutine = Timing.RunCoroutine(CoroutineMethod()));
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            lock (_playerDisplayListLock)
            {
                PlayerDisplay pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

                if (pd is null)
                    return;

                MainThreadDispatcher.Dispatch(() => Timing.KillCoroutines(pd._coroutine));
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

                //If player has quit, then stop the coroutine
                if (this.ReferenceHub == null)
                    break;

                //Reset the success flag
                bool isSuccessful = true;

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
                    isSuccessful = false; //If error occurred, set the success flag to false
                }

                //If the update is not successful, wait for a while before trying again so that it will not stuck the log.
                if (!isSuccessful)
                {
                    yield return Timing.WaitForSeconds(1f);
                }
            }
        }

        private void OnHintUpdate(object sender, PropertyChangedEventArgs ev)
        {
            if (!(sender is AbstractHint hint))
                return;

            //Skip if the hint's property changed when it is hided
            if (ev.PropertyName != "Hide" && hint.Hide)
                return;

            if (hint.SyncSpeed == HintSyncSpeed.UnSync)
                return;

            float maxWaitingTime;
            switch (hint.SyncSpeed)
            {
                case HintSyncSpeed.Fastest:
                    maxWaitingTime = 0;
                    break;
                case HintSyncSpeed.Fast:
                    maxWaitingTime = 0.1f;
                    break;
                case HintSyncSpeed.Normal:
                    maxWaitingTime = 0.3f;
                    break;
                case HintSyncSpeed.Slow:
                    maxWaitingTime = 1f;
                    break;
                case HintSyncSpeed.Slowest:
                    maxWaitingTime = 3f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ScheduleUpdate(maxWaitingTime, hint);
        }

        private void ScheduleUpdate(float maxWaitingTime = float.MinValue, AbstractHint updatingHint = null)
        {
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
                if (!(_currentParserTask is null))
                    return;

                _currentParserTask =
                    Task.Run(() =>
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

                if (!(existing is null))
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

        /// <summary>
        /// Return the first hin that match the id
        /// </summary>
        public AbstractHint GetHint(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to GetHint");

            return InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id).FirstOrDefault();
        }

        public IEnumerable<AbstractHint> GetHints(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to GetHints");

            return InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id);
        }

        public AbstractHint GetHint(Guid id)
        {
            return InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == id).FirstOrDefault();
        }

        public IEnumerable<AbstractHint> GetHints()
        {
            return this.InternalGetHints(Assembly.GetCallingAssembly().FullName);
        }

        internal void InternalAddHint(string name, AbstractHint hint)
        {
            hint.PropertyChanged += OnHintUpdate;
            UpdateAvailable += hint.TryUpdateHint;

            _hints.AddHint(name, hint);
        }

        internal void InternalAddHint(string name, IEnumerable<AbstractHint> hints)
        {
            foreach (AbstractHint hint in hints)
            {
                hint.PropertyChanged += OnHintUpdate;
                UpdateAvailable += hint.TryUpdateHint;

                _hints.AddHint(name, hint);
            }
        }

        internal void InternalRemoveHint(string name, AbstractHint hint)
        {
            hint.PropertyChanged -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, hint);
        }

        internal void InternalRemoveHint(string name, IEnumerable<AbstractHint> hints)
        {
            foreach (AbstractHint hint in hints)
            {
                hint.PropertyChanged -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;

                _hints.RemoveHint(name, hint);
            }
        }

        internal void InternalRemoveHint(string name, Guid guid)
        {
            AbstractHint hint = _hints.GetHints(name).FirstOrDefault(x => x.Guid.Equals(guid));

            if (hint == null)
                return;

            hint.PropertyChanged -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, x => x.Guid.Equals(guid));
        }

        internal void InternalRemoveHint(string name, string id)
        {
            IEnumerable<AbstractHint> removeList = _hints.GetHints(name).Where(predicate => predicate.Id == id);

            foreach (AbstractHint hint in removeList)
            {
                hint.PropertyChanged -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            _hints.RemoveHint(name, x => x.Id.Equals(id));
        }

        internal void InternalClearHint(string name)
        {
            foreach (AbstractHint hint in _hints.GetHints(name).ToList())
            {
                hint.PropertyChanged -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            _hints.ClearHints(name);
        }

        internal IEnumerable<AbstractHint> InternalGetHints(string name)
        {
            return _hints.GetHints(name);
        }

        internal IEnumerable<AbstractHint> InternalGetHints(string name, Func<AbstractHint, bool> predicate)
        {
            return _hints.GetHints(name, predicate);
        }

        internal void ShowCompatibilityHint(string assemblyName, string content, float duration) => this._adapter.ShowHint(new CompatibilityAdaptorArg(assemblyName, content, duration));

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
