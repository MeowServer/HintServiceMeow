using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Hints;
using Mirror;
using MEC;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Models;

//Plugin API
using Log = PluginAPI.Core.Log;


namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Represent a player's display. This class is used to manage hints and update hint to player's display
    /// </summary>
    public class PlayerDisplay
    {
        private static readonly object PlayerDisplayLock = new object();

        /// <summary>
        /// Instance Trackers
        /// </summary>
        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = new HashSet<PlayerDisplay>();

        private static readonly TextHint HintTemplate = new TextHint("", new HintParameter[] { new StringHintParameter("") }, new HintEffect[] { HintEffectPresets.TrailingPulseAlpha(1, 1, 1) }, float.MaxValue);

        /// <summary>
        /// Groups of hints shows to the ReferenceHub. First group is used for regular hints, reset of the groups are used for compatibility hints
        /// </summary>
        private readonly HintCollection _hints = new HintCollection();

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

        /// <summary>
        /// Coroutine that implements force update and UpdateAvailable event functions
        /// </summary>
        private static readonly CoroutineHandle Coroutine = Timing.RunCoroutine(CoroutineMethod());

        private static readonly CoroutineHandle UpdateCoroutine = Timing.RunCoroutine(UpdateCoroutineMethod());

        private readonly HintParser _hintParser = new HintParser();

        private Task<string> _currentParserTask;
        private readonly object _currentParserTaskLock = new object();

        #region Update Rate Management

        private readonly object _rateDataLock = new object();

        /// <summary>
        /// The text that was sent to the client in latest sync
        /// </summary>
        private string _lastText = string.Empty;

        /// <summary>
        /// Contains all the hints that had been arranged to update. Make sure that a hint's update time will not be calculated for twice
        /// </summary>
        private readonly HashSet<AbstractHint> _updatingHints = new HashSet<AbstractHint>();

        /// <summary>
        /// The time of latest update
        /// </summary>
        private DateTime _lastTimeUpdate = DateTime.MinValue;

        /// <summary>
        /// The time of second-last update
        /// </summary>
        private DateTime _secondLastTimeUpdate = DateTime.MinValue;

        /// <summary>
        /// The time of next update
        /// </summary>
        private DateTime _planUpdateTime = DateTime.MaxValue;

        /// <summary>
        /// The time when the player display will plan for an update
        /// </summary>
        private DateTime _arrangedUpdateTime = DateTime.MaxValue;

        /// <summary>
        /// The minimum interval between a regular update and the last update
        /// </summary>
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5f);

        /// <summary>
        /// The minimum interval between a force update and the second last update
        /// </summary>
        private static readonly TimeSpan SecondLastUpdateInterval = TimeSpan.FromSeconds(1f);

        /// <summary>
        /// The player display will be forced to update when there haven't been any update in this amount of time
        /// </summary>
        private static readonly TimeSpan PeriodicUpdateInterval = TimeSpan.FromSeconds(5f);

        /// <summary>
        /// The time left for regular update to cool down.
        /// </summary>
        private TimeSpan UpdateCoolDown
        {
            get
            {
                TimeSpan delay1 = (this._lastTimeUpdate + UpdateInterval - DateTime.Now);
                TimeSpan delay2 = (this._secondLastTimeUpdate + SecondLastUpdateInterval - DateTime.Now);
                return delay1 > delay2 ? delay1 : delay2;
            }
        }

        /// <summary>
        /// The time left for fast update to cool down.
        /// </summary>
        private TimeSpan FastUpdateCoolDown => this._secondLastTimeUpdate + SecondLastUpdateInterval - DateTime.Now;

        /// <summary>
        /// The time PlayerDisplay will execute a periodic update
        /// </summary>
        private TimeSpan PeriodicUpdateCoolDown => _lastTimeUpdate + PeriodicUpdateInterval - DateTime.Now;

        /// <summary>
        /// Whether PlayerDisplay need to force update or not
        /// </summary>
        private bool NeedPeriodicUpdate => PeriodicUpdateCoolDown.Ticks <= 0;

        /// <summary>
        /// Whether regular update is ready
        /// </summary>
        private bool UpdateReady => UpdateCoolDown.Ticks <= 0;

        /// <summary>
        /// Whether fast update is ready
        /// </summary>
        private bool FastUpdateReady => FastUpdateCoolDown.Ticks <= 0;

        internal void OnHintUpdate(AbstractHint hint)
        {
            lock (_rateDataLock)
            {
                if (_updatingHints.Contains(hint))
                    return;

                _updatingHints.Add(hint);
            }

            switch (hint.SyncSpeed)
            {
                case HintSyncSpeed.Fastest:
                    UpdateWhenAvailable(true);
                    break;
                case HintSyncSpeed.Fast:
                    UpdateWhenAvailable();
                    break;
                case HintSyncSpeed.Normal:
                    Task.Run(() => ArrangeUpdate(TimeSpan.FromSeconds(0.3f), hint));
                    break;
                case HintSyncSpeed.Slow:
                    Task.Run(() => ArrangeUpdate(TimeSpan.FromSeconds(1f), hint));
                    break;
                case HintSyncSpeed.Slowest:
                    Task.Run(() => ArrangeUpdate(TimeSpan.FromSeconds(3f), hint));
                    break;
                case HintSyncSpeed.UnSync:
                    break;
            }
        }

        /// <summary>
        /// Plan an update when an update chance is available.
        /// </summary>
        private void UpdateWhenAvailable(bool useFastUpdate = false)
        {
            lock (_rateDataLock)
            {
                TimeSpan timeToWait = useFastUpdate ? FastUpdateCoolDown : UpdateCoolDown;

                var newPlanUpdateTime = DateTime.Now + timeToWait + TimeSpan.FromMilliseconds(5);

                if (_planUpdateTime > newPlanUpdateTime)
                    _planUpdateTime = newPlanUpdateTime;
            }
        }

        /// <summary>
        /// Call UpdateWhenAvailable method based on estimated next update time of other hints
        /// </summary>
        private void ArrangeUpdate(TimeSpan maxDelay, AbstractHint hint)
        {
            try
            {
                var now = DateTime.Now;

                //Find the latest estimated update time within maxDelay
                DateTime newTime = _hints.AllHints
                    .Where(h => h.SyncSpeed >= hint.SyncSpeed && h != hint)
                    .Select(h => h.Analyser.EstimateNextUpdate())
                    .Where(x => x - now >= TimeSpan.Zero && x - now <= maxDelay)
                    .DefaultIfEmpty(DateTime.Now)
                    .Max();

                lock (_rateDataLock)
                {
                    if (_arrangedUpdateTime > newTime)
                        _arrangedUpdateTime = newTime;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private static IEnumerator<float> CoroutineMethod()
        {
            while (true)
            {
                try
                {
                    lock (PlayerDisplayLock)
                    {
                        foreach (PlayerDisplay pd in PlayerDisplayList)
                        {
                            lock (pd._rateDataLock)
                            {
                                //Force update
                                if (pd.NeedPeriodicUpdate && pd._currentParserTask == null)
                                    pd.StartParserTask();

                                //Invoke UpdateAvailable event
                                if (pd.UpdateReady)
                                    pd.UpdateAvailable?.Invoke(new UpdateAvailableEventArg(pd));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private static IEnumerator<float> UpdateCoroutineMethod()
        {
            while (true)
            {
                try
                {
                    DateTime now = DateTime.Now;

                    lock (PlayerDisplayLock)
                    {
                        foreach (PlayerDisplay pd in PlayerDisplayList)
                        {
                            lock (pd._rateDataLock)
                            {
                                //Check arranged update time
                                if (now > pd._arrangedUpdateTime)
                                {
                                    pd._arrangedUpdateTime = DateTime.MaxValue;
                                    pd.UpdateWhenAvailable();
                                }

                                //Update based on plan
                                //If last update complete, then start a new update. Otherwise, wait for the last update to complete
                                if (now > pd._planUpdateTime && pd._currentParserTask == null)
                                {
                                    //Reset Update Plan
                                    pd._planUpdateTime = DateTime.MaxValue;
                                    pd._updatingHints.Clear();

                                    pd.StartParserTask();
                                }
                            }

                            if (pd._currentParserTask != null && pd._currentParserTask.IsCompleted)
                            {
                                var text = pd._currentParserTask.GetAwaiter().GetResult();
                                bool shouldUpdate = false;

                                lock (pd._rateDataLock)
                                {
                                    //Check whether the text had changed since last update or if this is a force update
                                    if (text != pd._lastText || pd.NeedPeriodicUpdate)
                                    {
                                        //Update text record
                                        pd._lastText = text;

                                        //Reset CountDown
                                        pd._secondLastTimeUpdate = pd._lastTimeUpdate;
                                        pd._lastTimeUpdate = DateTime.Now;

                                        shouldUpdate = true;
                                    }
                                }

                                if (shouldUpdate)
                                    pd.SendHint(text);

                                pd._currentParserTask.Dispose();
                                pd._currentParserTask = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Force an update when the update is available. You do not need to use this method unless you are using UnSync hints
        /// </summary>
        /// <param name="useFastUpdate">Use fast update might increase the cooldown of next update</param>
        public void ForceUpdate(bool useFastUpdate = false)
        {
            UpdateWhenAvailable(useFastUpdate);
        }

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
                ConnectionToClient.Send(new HintMessage(HintTemplate));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        #endregion

        #region Constructor and Destructors

        private PlayerDisplay(ReferenceHub referenceHub)
        {
            this.ConnectionToClient = referenceHub.netIdentity.connectionToClient;
            this.ReferenceHub = referenceHub;

            lock (PlayerDisplayLock)
                PlayerDisplayList.Add(this);
        }

        internal static PlayerDisplay TryCreate(ReferenceHub referenceHub)
        {
            PlayerDisplay pd;

            lock (PlayerDisplayLock)
                pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return pd ?? new PlayerDisplay(referenceHub);
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            lock (PlayerDisplayLock)
                PlayerDisplayList.RemoveWhere(x => x.ReferenceHub == referenceHub);
        }

        internal static void ClearInstance()
        {
            lock (PlayerDisplayLock)
                PlayerDisplayList.Clear();
        }

        #endregion

        #region Player Display Methods

        /// <summary>
        /// Get the PlayerDisplay instance of the player
        /// </summary>
        public static PlayerDisplay Get(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new Exception("A null ReferenceHub had been passed to Get method");

            PlayerDisplay pd;

            lock (PlayerDisplayLock)
                pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return pd ?? new PlayerDisplay(referenceHub);//TryCreate ReferenceHub display if it has not been created yet
        }

#if EXILED
        /// <summary>
        /// Get the PlayerDisplay instance of the player
        /// </summary>
        public static PlayerDisplay Get(Exiled.API.Features.Player player)
        {
            if(player is null)
                throw new Exception("A null player had been passed to Get method");

            return Get(player.ReferenceHub);
        }
#endif

        #endregion

        #region Hint Methods

        public void AddHint(AbstractHint hint)
        {
            if (hint == null)
                return;

            var name = Assembly.GetCallingAssembly().FullName;

            this.InternalAddHint(name, hint);
        }

        public void AddHint(IEnumerable<AbstractHint> hints)
        {
            if (hints == null)
                return;

            var name = Assembly.GetCallingAssembly().FullName;

            this.InternalAddHint(name, hints);
        }

        public void RemoveHint(AbstractHint hint)
        {
            if (hint == null)
                return;

            var name = Assembly.GetCallingAssembly().FullName;

            this.InternalRemoveHint(name, hint);
        }

        public void RemoveHint(IEnumerable<AbstractHint> hints)
        {
            if (hints == null)
                return;

            var name = Assembly.GetCallingAssembly().FullName;

            this.InternalRemoveHint(name, hints);
        }

        public void RemoveHint(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("A null or a empty ID had been passed to RemoveFromHintList");

            var name = Assembly.GetCallingAssembly().FullName;

            this.InternalRemoveHint(name, id);
        }

        public void ClearHint()
        {
            var name = Assembly.GetCallingAssembly().FullName;

            this.InternalClearHint(name);
        }

        public AbstractHint GetHint(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("A null or a empty name had been passed to GetHint");

            var name = Assembly.GetCallingAssembly().FullName;

            return _hints.GetHints(name).FirstOrDefault(x => x.Id == id);
        }

        internal void InternalAddHint(string name, AbstractHint hint)
        {
            hint.HintUpdated += OnHintUpdate;
            UpdateAvailable += hint.TryUpdateHint;

            _hints.AddHint(name, hint);

            UpdateWhenAvailable();
        }

        internal void InternalAddHint(string name, IEnumerable<AbstractHint> hints)
        {
            foreach (var hint in hints)
            {
                hint.HintUpdated += OnHintUpdate;
                UpdateAvailable += hint.TryUpdateHint;

                _hints.AddHint(name, hint);
            }

            UpdateWhenAvailable();
        }

        internal void InternalRemoveHint(string name, AbstractHint hint)
        {
            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, hint);

            UpdateWhenAvailable();
        }

        internal void InternalRemoveHint(string name, IEnumerable<AbstractHint> hints)
        {
            foreach (var hint in hints)
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;

                _hints.RemoveHint(name, hint);
            }

            UpdateWhenAvailable();
        }

        internal void InternalRemoveHint(string name, string id)
        {
            var hint = _hints.GetHints(name).FirstOrDefault(x => x.Id.Equals(id));

            if (hint == null)
                return;

            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, x => x.Id.Equals(id));

            UpdateWhenAvailable();
        }

        internal void InternalClearHint(string name)
        {
            foreach(var hint in _hints.GetHints(name))
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            _hints.ClearHints(name);

            UpdateWhenAvailable();
        }

        #endregion

        #region Extension Methods
        public void RemoveAfter(AbstractHint hint, float seconds)
        {
            var name = Assembly.GetCallingAssembly().FullName;
            Timing.CallDelayed(seconds, () => InternalRemoveHint(name, hint));    
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
