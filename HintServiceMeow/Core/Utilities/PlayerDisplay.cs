using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Mirror;
using MEC;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Utilities.Parser;

//Plugin API
using PluginAPI.Core;

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
        /// Invoke every frame when ReferenceHub display is ready to update.
        /// </summary>
        public event UpdateAvailableEventHandler UpdateAvailable;

        public delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev);

        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = new HashSet<PlayerDisplay>();
        private static readonly object PlayerDisplayListLock = new object();

        private static readonly Hints.TextHint HintTemplate = new Hints.TextHint("", new Hints.HintParameter[] { new Hints.StringHintParameter("") }, new Hints.HintEffect[] { Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1) }, float.MaxValue);

        private readonly HintCollection _hints = new HintCollection();
        private readonly HintParser _hintParser = new HintParser();
        private readonly TaskScheduler _taskScheduler;

        internal readonly CompatibilityAdaptor Adapter;

        private readonly CoroutineHandle _updateCoroutine;
        private readonly CoroutineHandle _coroutine;

        private readonly HashSet<AbstractHint> _updatingHints = new HashSet<AbstractHint>();

        private Task<string> _currentParserTask;
        private readonly object _currentParserTaskLock = new object();

        #region Constructor and Destructors

        private PlayerDisplay(ReferenceHub referenceHub)
        {
            this.ConnectionToClient = referenceHub.netIdentity.connectionToClient;
            this.ReferenceHub = referenceHub;

            this._taskScheduler = new TaskScheduler(TimeSpan.FromSeconds(0.5f), () =>
            {
                StartParserTask();
                _taskScheduler?.PauseAction();//Pause action until the parser task is done
            });
            this.Adapter = new CompatibilityAdaptor(this);

            _updateCoroutine = Timing.RunCoroutine(UpdateCoroutineMethod());
            _coroutine = Timing.RunCoroutine(CoroutineMethod());

            lock (PlayerDisplayListLock)
                PlayerDisplayList.Add(this);
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            lock (PlayerDisplayListLock)
            {
                foreach (var pd in PlayerDisplayList)
                {
                    if (pd.ReferenceHub != referenceHub)
                        continue;

                    Timing.KillCoroutines(pd._updateCoroutine);
                    Timing.KillCoroutines(pd._coroutine);
                }

                PlayerDisplayList.RemoveWhere(x => x.ReferenceHub == referenceHub);
            }
        }

        internal static void ClearInstance()
        {
            lock (PlayerDisplayListLock)
                PlayerDisplayList.Clear();
        }

        #endregion

        #region _coroutine Methods
        private IEnumerator<float> UpdateCoroutineMethod()
        {
            //Basically send hint when parser task is done
            while (true)
            {
                lock(_currentParserTaskLock)
                {
                    while(_currentParserTask is null || !_currentParserTask.IsCompleted)
                        yield return Timing.WaitForOneFrame;

                    try
                    {
                        SendHint(_currentParserTask.Result);

                        _taskScheduler.ResumeAction();

                        _currentParserTask.Dispose();
                        _currentParserTask = null;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }

                    yield return Timing.WaitForOneFrame;
                }
            }
        }

        private IEnumerator<float> CoroutineMethod()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(0.1f);

                if (_taskScheduler.IsReadyForNextAction())
                {
                    try
                    {
                        UpdateAvailable?.Invoke(new UpdateAvailableEventArg(this));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }

                //Periodic update
                if (_taskScheduler.LastActionTime + TimeSpan.FromSeconds(5) < DateTime.Now)
                    ScheduleUpdate();
            }
        }
        #endregion

        #region Update Management
        internal void OnHintUpdate(AbstractHint hint)
        {
            if(_updatingHints.Contains(hint))
                return;

            switch (hint.SyncSpeed)
            {
                case HintSyncSpeed.Fastest:
                    ScheduleUpdate();
                    break;
                case HintSyncSpeed.Fast:
                    ScheduleUpdate(0.1f, hint);
                    break;
                case HintSyncSpeed.Normal:
                    ScheduleUpdate(0.3f, hint);
                    break;
                case HintSyncSpeed.Slow:
                    ScheduleUpdate(1f, hint);
                    break;
                case HintSyncSpeed.Slowest:
                    ScheduleUpdate(3f, hint);
                    break;
                case HintSyncSpeed.UnSync:
                    break;
            }

            _updatingHints.Add(hint);
            Timing.CallDelayed(Timing.WaitForOneFrame, () => _updatingHints.Remove(hint));//Suppress the hint from scheduling update for one frame
        }

        private void ScheduleUpdate(float maxWaitingTime = float.MinValue, AbstractHint updatingHint = null)
        {
            lock(_currentParserTaskLock)
            {
                if (_currentParserTask != null)
                    return;
            }

            if (maxWaitingTime <= 0)
            {
                _taskScheduler.StartAction();
                return;
            }

            var predictingHints = _hints.AllHints;

            if(updatingHint != null)
            {
                predictingHints = predictingHints.Where(h => h.SyncSpeed >= updatingHint.SyncSpeed && h != updatingHint);
            }

            var maxWaitingTimeSpan = TimeSpan.FromSeconds(maxWaitingTime);
            var now = DateTime.Now;

            DateTime newTime = predictingHints
                .Select(h => h.Analyser.EstimateNextUpdate())
                .Where(x => x - now >= TimeSpan.Zero && x - now <= maxWaitingTimeSpan)
                .DefaultIfEmpty(DateTime.Now)
                .Max();

            var delay = (float)(newTime - DateTime.Now).TotalSeconds;

            if(delay <= 0)
                _taskScheduler.StartAction();
            else
                _taskScheduler.StartAction(delay, TaskScheduler.DelayType.Fastest);
        }

        /// <summary>
        /// Force an update when the update is available. You do not need to use this method unless you are using UnSync hints
        /// </summary>
        public void ForceUpdate(bool useFastUpdate = false)
        {
            ScheduleUpdate(useFastUpdate ? 0f : 0.3f);
        }
        #endregion

        #region Update Methods
        private void StartParserTask()
        {
            lock (_currentParserTaskLock)
                _currentParserTask = Task.Run(() => _hintParser.GetMessage(_hints));
        }

        private void SendHint(string text)
        {
            try
            {
                if(ConnectionToClient == null || !ConnectionToClient.isReady)
                    return;

                HintTemplate.Text = text;
                ConnectionToClient.Send(new Hints.HintMessage(HintTemplate));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        #endregion

        #region Player Display Methods

        /// <summary>
        /// Get the PlayerDisplay instance of the player. If the instance have not been created yet, then it will create one.
        /// Not Thread Safe
        /// </summary>
        public static PlayerDisplay Get(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new ArgumentNullException(nameof(referenceHub));

            lock (PlayerDisplayListLock)
                return PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub) ?? new PlayerDisplay(referenceHub);//TryCreate ReferenceHub display if it has not been created yet
        }

        /// <summary>
        /// Get the PlayerDisplay instance of the player. If the instance have not been created yet, then it will create one.
        /// Not Thread Safe
        /// </summary>
        public static PlayerDisplay Get(Player player)
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
            if(player is null)
                throw new ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }
#endif

        #endregion

        #region Hint Methods

        public void AddHint(AbstractHint hint)
        {
            if (hint == null)
                return;

            this.InternalAddHint(Assembly.GetCallingAssembly().FullName, hint);
        }

        public void AddHint(IEnumerable<AbstractHint> hints)
        {
            if (hints == null)
                return;

            this.InternalAddHint(Assembly.GetCallingAssembly().FullName, hints);
        }

        public void RemoveHint(AbstractHint hint)
        {
            if (hint == null)
                return;

            this.InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);
        }

        public void RemoveHint(IEnumerable<AbstractHint> hints)
        {
            if (hints == null)
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

            var name = Assembly.GetCallingAssembly().FullName;

            return _hints.GetHints(name).FirstOrDefault(x => x.Id == id);
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
            foreach (var hint in hints)
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
            foreach (var hint in hints)
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;

                _hints.RemoveHint(name, hint);
            }

            if (update)
                ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, string id, bool update = true)
        {
            var hint = _hints.GetHints(name).FirstOrDefault(x => x.Id.Equals(id));

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
            foreach(var hint in _hints.GetHints(name).ToList())
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            _hints.ClearHints(name);

            if (update)
                ScheduleUpdate();
        }

        #endregion

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
