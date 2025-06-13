using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Tools;
using MEC;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Represent a player's display. This class is used to manage hints and update hint to player's display
    /// </summary>
    public class PlayerDisplay : IPlayerDisplay, Interface.IDestructible
    {
        /// <summary>
        /// The player this instance bind to
        /// </summary>
        public ReferenceHub ReferenceHub => _playerContext is ReferenceHubContext context ? context.ReferenceHub : null;

        /// <summary>
        /// Invoke every tick when ReferenceHub display is ready to update.
        /// </summary>
        public event UpdateAvailableEventHandler UpdateAvailable;
        public delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev);

        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = new HashSet<PlayerDisplay>();
        private static readonly object PlayerDisplayListLock = new object();

        private readonly List<IDisplayOutput> _displayOutputs = new();
        private readonly object _displayOutputsLock = new object();

        private readonly IPlayerContext _playerContext;
        private readonly HintCollection _hints = new HintCollection();
        private readonly ITaskScheduler _updateScheduler;//Initialize in constructor
        private IHintParser _hintParser = new HintParser();
        private ICompatibilityAdaptor _adapter;//Initialize in constructor

        private CoroutineHandle _coroutine;//Initialize in constructor

        private Task _currentParserTask;
        private readonly object _currentParserTaskLock = new object();

        private PlayerDisplay(
            IPlayerContext playerContext,
            HintCollection hints = null,
            ITaskScheduler updateScheduler = null,
            ICompatibilityAdaptor adaptor = null,
            IHintParser hintParser = null,
            IEnumerable<IDisplayOutput> displayOutputs = null)
        {
            _playerContext = playerContext ?? throw new ArgumentNullException(nameof(playerContext));

            if (hints != null)
                _hints = hints;
            if (hintParser != null)
                _hintParser = hintParser;
            if (displayOutputs != null)
                _displayOutputs = displayOutputs?.ToList();

            _adapter = adaptor ?? new CompatibilityAdaptor(this); // Default compatibility adaptor
            _updateScheduler = updateScheduler ?? new TaskScheduler(); // Default task scheduler with zero interval

            _hints.CollectionChanged += OnCollectionChanged;
            _updateScheduler.Start(TimeSpan.Zero, () =>
            {
                _updateScheduler.Pause();//Pause action until the parser task is finishing
                StartParserTask();
            });
            MainThreadDispatcher.Dispatch(() => this._coroutine = Timing.RunCoroutine(CoroutineMethod()));
        }

        private PlayerDisplay(ReferenceHub referenceHub)
            : this(new ReferenceHubContext(referenceHub))
        {
            this._displayOutputs.Add(new DefaultDisplayOutput(referenceHub.connectionToClient));
        }

        /// <summary>
        /// Not thread safe
        /// </summary>
        internal static void Destruct(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new ArgumentNullException(nameof(referenceHub));

            ReferenceHubContext context = new ReferenceHubContext(referenceHub);

            lock (PlayerDisplayListLock)
            {
                PlayerDisplay pd = PlayerDisplayList.FirstOrDefault(x => x._playerContext.Equals(context));

                if (pd is null)
                    return;

                ((Interface.IDestructible)pd).Destruct();

                PlayerDisplayList.Remove(pd); // Remove from the reference list
            }
        }

        void Interface.IDestructible.Destruct()
        {
            Timing.KillCoroutines(this._coroutine); // Stop coroutine
            this.UpdateAvailable = null; // Clear event

            // Clear collection's reference to this pd
            this._hints.CollectionChanged -= this.OnCollectionChanged;

            // Clear hint's reference to this pd
            foreach (var hint in this._hints.GetHints(null))
            {
                hint.PropertyChanged -= this.OnHintUpdate;
                this.UpdateAvailable -= hint.TryUpdateHint;
            }

            // Clear pd's reference to hints
            this._hints.ClearHints(null);

            ((Interface.IDestructible)this._updateScheduler).Destruct(); // Stop task scheduler's coroutine

            ((Interface.IDestructible)this._adapter).Destruct(); // Stop compatibility adaptor's coroutine
        }

        internal static void ClearInstance()
        {
            lock (PlayerDisplayListLock)
            {
                foreach (PlayerDisplay pd in PlayerDisplayList)
                {
                    if (pd is null)
                        continue;

                    ((Interface.IDestructible)pd).Destruct();
                }

                PlayerDisplayList.Clear();
            }
        }

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
                if (!this._playerContext.IsValid())
                    break;

                //Reset the success flag
                bool isSuccessful = true;

                try
                {
                    //Periodic update
                    if (_updateScheduler.Elapsed > TimeSpan.FromSeconds(5))
                        ScheduleUpdate();

                    if (_updateScheduler.IsReadyForNextAction)
                    {
                        UpdateAvailable?.Invoke(new UpdateAvailableEventArg(this));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                    isSuccessful = false; //If error occurred, set the success flag to false
                }

                //If the update is not successful, wait for a while before trying again so that it will not stuck the log.
                if (!isSuccessful)
                {
                    yield return Timing.WaitForSeconds(1f);
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScheduleUpdate();
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
                _updateScheduler.Invoke();
                return;
            }

            IEnumerable<AbstractHint> predictingHints = _hints.AllGroups.SelectMany(x => x);

            if (updatingHint != null)
            {
                predictingHints = predictingHints.Where(h => h.SyncSpeed >= updatingHint.SyncSpeed && h != updatingHint);
            }

            TimeSpan maxWaitingTimeSpan = TimeSpan.FromSeconds(maxWaitingTime);
            DateTime now = DateTime.Now;

            DateTime delayedUpdateTime = predictingHints
                .Select(h => h.UpdateAnalyser.EstimateNextUpdate())
                .Where(x => x - now >= TimeSpan.Zero && x - now <= maxWaitingTimeSpan)
                .DefaultIfEmpty(now)
                .Max();

            float delay = (float)(delayedUpdateTime - now).TotalSeconds;
            delay = Math.Max(maxWaitingTime, delay * 1.1f); //Increase by 10% to make increase hit rate of prediction

            if (delay <= 0)
                _updateScheduler.Invoke();
            else
                _updateScheduler.Invoke(delay, DelayType.KeepFastest);
        }

        /// <summary>
        /// Force an update when the update is available. You do not have to use this method unless you are using HintSyncSpeed.UnSync
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
                    ConcurrentTaskDispatcher.Instance.Enqueue(async () =>
                    {
                        string richText;

                        try
                        {
                            richText = _hintParser.ParseToMessage(_hints);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error(ex);
                            return Task.CompletedTask;
                        }

                        MainThreadDispatcher.Dispatch(() =>
                        {
                            try
                            {
                                SendHint(richText);
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance.Error(ex);
                            }
                            finally
                            {
                                _updateScheduler.Resume(); //Resume action after the parser task is finishing

                                lock (_currentParserTaskLock)
                                {
                                    _currentParserTask = null;
                                }
                            }
                        });

                        return Task.CompletedTask;
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
                    Logger.Instance.Error(ex);
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

            ReferenceHubContext context = new ReferenceHubContext(referenceHub);

            lock (PlayerDisplayListLock)
            {
                PlayerDisplay existing = PlayerDisplayList.FirstOrDefault(x => x._playerContext.Equals(context));

                if (!(existing is null))
                    return existing;

                PlayerDisplay newPlayerDisplay = new(referenceHub);
                PlayerDisplayList.Add(newPlayerDisplay);
                return newPlayerDisplay;
            }
        }

        /// <summary>
        /// Get the PlayerDisplay instance of the player. If the instance have not been created yet, then it will create one.
        /// Not Thread Safe
        /// </summary>
        public static PlayerDisplay Get(LabApi.Features.Wrappers.Player player)
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
        public void AddDisplayOutput(IDisplayOutput output)
        {
            lock (_displayOutputsLock)
            {
                _displayOutputs.Add(output);
            }
        }

        public void RemoveDisplayOutput(IDisplayOutput output)
        {
            lock (_displayOutputsLock)
            {
                _displayOutputs.Remove(output);
            }
        }

        public void RemoveDisplayOutput<T>() where T : IDisplayOutput
        {
            lock (_displayOutputsLock)
            {
                _displayOutputs.RemoveAll(x => x is T);
            }
        }

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
        /// Return the first hint that match the id
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

        /// <summary>
        /// Return the first hint that match the guid
        /// </summary>
        public AbstractHint GetHint(Guid guid)
        {
            return InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == guid).FirstOrDefault();
        }

        public IEnumerable<AbstractHint> GetHints()
        {
            return this.InternalGetHints(Assembly.GetCallingAssembly().FullName);
        }

        public bool HasHint(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to HasHint");

            return InternalGetHints(Assembly.GetCallingAssembly().FullName, hint => hint.Id == id).Any();
        }

        public bool HasHint(Guid guid)
        {
            return InternalGetHints(Assembly.GetCallingAssembly().FullName, hint => hint.Guid == guid).Any();
        }

        /// <summary>
        /// Return the first hint that match the id
        /// </summary>
        public bool TryGetHint(string id, out AbstractHint hint)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to TryGetHint");

            hint = InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id).FirstOrDefault();

            return hint != null;
        }

        /// <summary>
        /// Return the first hint that match the guid
        /// </summary>
        public bool TryGetHint(Guid guid, out AbstractHint hint)
        {
            hint = InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == guid).FirstOrDefault();
            return hint != null;
        }

        public bool TryGetHints(string id, out IEnumerable<AbstractHint> hints)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to TryGetHints");

            hints = InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id);
            return hints.Any();
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

        internal IReadOnlyList<AbstractHint> InternalGetHints(string name)
        {
            return _hints.GetHints(name);
        }

        internal IReadOnlyList<AbstractHint> InternalGetHints(string name, Func<AbstractHint, bool> predicate)
        {
            return _hints.GetHints(name, predicate);
        }

        internal void ShowCompatibilityHint(string assemblyName, string content, float duration) => this._adapter.ShowHint(new CompatibilityAdaptorArg(assemblyName, content, duration));
    }
}