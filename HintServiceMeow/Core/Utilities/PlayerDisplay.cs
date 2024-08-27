using System;
using System.Collections.Generic;
using System.Linq;

using Hints;

using MEC;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;

//Plugin API
using Log = PluginAPI.Core.Log;

namespace HintServiceMeow.Core.Utilities
{
    public class PlayerDisplay
    {
        /// <summary>
        /// Instance Trackers
        /// </summary>
        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = new HashSet<PlayerDisplay>();

        private static readonly TextHint HintTemplate = new TextHint("", new HintParameter[] { new StringHintParameter("") }, new HintEffect[] { HintEffectPresets.TrailingPulseAlpha(1, 1, 1) }, float.MaxValue);

        /// <summary>
        /// List of hints shows to the ReferenceHub
        /// </summary>
        private readonly HashSet<AbstractHint> _hintList = new HashSet<AbstractHint>();//List of hints shows to the ReferenceHub

        /// <summary>
        /// The player this instance bind to
        /// </summary>
        public ReferenceHub ReferenceHub { get; }

        /// <summary>
        /// Invoke every frame when ReferenceHub display is ready to update.
        /// </summary>
        public event UpdateAvailableEventHandler UpdateAvailable;

        public delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev);

        /// <summary>
        /// Coroutine that implements force update and UpdateAvailable event functions
        /// </summary>
        private static CoroutineHandle _coroutine;

        private static CoroutineHandle _updateCoroutine;

        #region Update Rate Management

        /// <summary>
        /// The text that was sent to the client in latest sync
        /// </summary>
        internal string _lastText = string.Empty;

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
            if (_updatingHints.Contains(hint))
                return;

            _updatingHints.Add(hint);

            switch (hint.SyncSpeed)
            {
                case HintSyncSpeed.Fastest:
                    UpdateWhenAvailable(true);
                    break;
                case HintSyncSpeed.Fast:
                    UpdateWhenAvailable();
                    break;
                case HintSyncSpeed.Normal:
                    ArrangeUpdate(TimeSpan.FromSeconds(0.3f), hint);
                    break;
                case HintSyncSpeed.Slow:
                    ArrangeUpdate(TimeSpan.FromSeconds(1f), hint);
                    break;
                case HintSyncSpeed.Slowest:
                    ArrangeUpdate(TimeSpan.FromSeconds(3f), hint);
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
            TimeSpan timeToWait = useFastUpdate ? FastUpdateCoolDown : UpdateCoolDown;

            var newPlanUpdateTime = DateTime.Now + timeToWait;

            if (_planUpdateTime > newPlanUpdateTime)
                _planUpdateTime = newPlanUpdateTime;
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
                IEnumerable<DateTime> estimatedTime = _hintList
                    .Where(x => x.SyncSpeed >= hint.SyncSpeed && x != hint)
                    .Select(x => x.Analyser.EstimateNextUpdate())
                    .Where(x => x - now >= TimeSpan.Zero && x - now <= maxDelay)
                    .DefaultIfEmpty(DateTime.Now);

                DateTime newTime = estimatedTime.Max();

                if (_arrangedUpdateTime > newTime)
                    _arrangedUpdateTime = newTime;
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
                    foreach (PlayerDisplay pd in PlayerDisplayList)
                    {
                        //Force update
                        if (pd.NeedPeriodicUpdate)
                            pd.UpdateHint(true);

                        //Invoke UpdateAvailable event
                        if (pd.UpdateReady)
                            pd.UpdateAvailable?.Invoke(new UpdateAvailableEventArg(pd));
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

                    foreach (PlayerDisplay pd in PlayerDisplayList)
                    {
                        //Check arranged update time
                        if (now > pd._arrangedUpdateTime)
                        {
                            pd._arrangedUpdateTime = DateTime.MaxValue;
                            pd.UpdateWhenAvailable(false);
                        }

                        //Update based on plan
                        if (now > pd._planUpdateTime)
                        {
                            pd._planUpdateTime = DateTime.MaxValue;
                            pd.UpdateHint();
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

        private void UpdateHint(bool isForceUpdate = false)
        {
            try
            {
                //Reset Update Plan
                _planUpdateTime = DateTime.MaxValue;
                _arrangedUpdateTime = DateTime.MaxValue;

                _updatingHints.Clear();

                string text = HintParser.GetMessage(_hintList, this);

                //Check whether the text had changed since last update or if this is a force update
                if (text == _lastText && !isForceUpdate)
                    return;

                //Update text record
                _lastText = text;

                //Reset CountDown
                _secondLastTimeUpdate = _lastTimeUpdate;
                _lastTimeUpdate = DateTime.Now;

                //Display the hint
                HintTemplate.Text = text;
                ReferenceHub.connectionToClient.Send(new HintMessage(HintTemplate));
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
            this.ReferenceHub = referenceHub;

            if (!_coroutine.IsRunning)
                _coroutine = Timing.RunCoroutine(CoroutineMethod());

            if (!_updateCoroutine.IsRunning)
                _updateCoroutine = Timing.RunCoroutine(UpdateCoroutineMethod());

            PlayerDisplayList.Add(this);
        }

        internal static PlayerDisplay TryCreate(ReferenceHub referenceHub)
        {
            var pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return pd ?? new PlayerDisplay(referenceHub);
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            PlayerDisplayList.RemoveWhere(x => x.ReferenceHub == referenceHub);
        }

        internal static void ClearInstance()
        {
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

            var pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

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
            
            hint.HintUpdated += OnHintUpdate;
            UpdateAvailable += hint.TryUpdateHint;

            _hintList.Add(hint);

            UpdateWhenAvailable();
        }

        public void AddHint(IEnumerable<AbstractHint> hints)
        {
            foreach(AbstractHint hint in hints)
            {
                AddHint(hint);
            }
        }

        public void RemoveHint(AbstractHint hint)
        {
            if (hint == null)
                return;

            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hintList.Remove(hint);

            UpdateWhenAvailable();
        }

        public void RemoveHint(IEnumerable<AbstractHint> hints)
        {
            foreach (var hint in hints)
            {
                RemoveHint(hint);
            }
        }

        public void RemoveHint(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("A null or a empty ID had been passed to RemoveHint");

            var toRemove = _hintList.Where(x => x.Id.Equals(id)).ToList();

            foreach (var hint in toRemove)
            {
                RemoveHint(hint);
            }
        }

        public AbstractHint GetHint(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("A null or a empty name had been passed to GetHint");

            return _hintList.FirstOrDefault(x => x.Id == id);
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
