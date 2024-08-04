using System;
using System.Collections.Generic;
using System.Linq;

using Hints;

using MEC;
using Mirror;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;

//Plugin API
using Log = PluginAPI.Core.Log;

//Exiled
using Player = Exiled.API.Features.Player;

namespace HintServiceMeow.Core.Utilities
{
    public class PlayerDisplay
    {
        /// <summary>
        /// Instance Trackers
        /// </summary>
        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = new HashSet<PlayerDisplay>();

        /// <summary>
        /// List of hints shows to the ReferenceHub
        /// </summary>
        private readonly HashSet<AbstractHint> _hintList = new HashSet<AbstractHint>();//List of hints shows to the ReferenceHub

        /// <summary>
        /// The player this instance bind to
        /// </summary>
        public ReferenceHub ReferenceHub { get; set; }

        /// <summary>
        /// Invoke periodically when ReferenceHub display is ready to update. Default interval is 0.1s (10 times per second)
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
        private DateTime _planUpdateTime = DateTime.MinValue;

        /// <summary>
        /// The time when the player display will plan for an update
        /// </summary>
        private DateTime _arrangedUpdateTime = DateTime.MinValue;

        /// <summary>
        /// The minimum interval between a regular update and the last update
        /// </summary>
        private static TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.5f);

        /// <summary>
        /// The minimum interval between a force update and the second last update
        /// </summary>
        private static TimeSpan SecondLastUpdateInterval => TimeSpan.FromSeconds(1f);

        /// <summary>
        /// The player display will be forced to update when there haven't been any update in this amount of time
        /// </summary>
        private static TimeSpan PeriodicUpdateInterval => TimeSpan.FromSeconds(5f);

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
        private bool NeedForceUpdate => PeriodicUpdateCoolDown.Ticks <= 0;

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
            if(_updatingHints.Contains(hint))
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
                    ArrangeUpdate(TimeSpan.FromSeconds(1f), hint);
                    break;
                case HintSyncSpeed.Slow:
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

            timeToWait = timeToWait.Milliseconds >= 25f ? timeToWait : TimeSpan.FromMilliseconds(25f);//Having a min delay to make sure that all the changes are done before next update
            var nextStartUpdateTime = DateTime.Now + timeToWait;

            if (nextStartUpdateTime < _planUpdateTime)
                _planUpdateTime = nextStartUpdateTime;
        }

        /// <summary>
        /// Call UpdateWhenAvailable method based on estimated next update time of other hints
        /// </summary>
        private void ArrangeUpdate(TimeSpan maxDelay, AbstractHint hint)
        {
            try
            {
                //Find the latest estimated update time within maxDelay
                List<DateTime> estimatedTime = _hintList
                    .Where(x => x.SyncSpeed >= hint.SyncSpeed)
                    .Where(x => x != hint)
                    .Select(x => x.Analyser.EstimateNextUpdate())
                    .Where(x => x - DateTime.Now >= TimeSpan.Zero)
                    .Where(x => x - DateTime.Now <= maxDelay)
                    .ToList();

                DateTime newTime;

                if (estimatedTime.IsEmpty())
                {
                    newTime = DateTime.Now;
                }
                else
                {
                    newTime = estimatedTime.Max();
                }

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
                        if (pd.NeedForceUpdate)
                        {
                            pd.UpdateWhenAvailable(false);
                        }

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
                    foreach (PlayerDisplay pd in PlayerDisplayList)
                    {
                        //Check arranged update time
                        if (DateTime.Now > pd._arrangedUpdateTime)
                        {
                            pd._arrangedUpdateTime = DateTime.MaxValue;
                            pd.UpdateWhenAvailable(false);
                        }

                        //Update based on plan
                        if (DateTime.Now > pd._planUpdateTime)
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

                yield return Timing.WaitForSeconds(0.025f);
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

        private void UpdateHint(bool IsForceUpdate = false)
        {
            try
            {
                //Check connection
                if (!NetworkServer.active || ReferenceHub.isLocalPlayer)
                    return;

                string text = HintParser.GetMessage(_hintList, this);

                //Reset CountDown
                _secondLastTimeUpdate = _lastTimeUpdate;
                _lastTimeUpdate = DateTime.Now;

                //Reset Update Plan
                _planUpdateTime = DateTime.MaxValue;
                _arrangedUpdateTime = DateTime.MaxValue;

                _updatingHints.Clear();

                //Check whether the text had changed since last update or if this is a force update
                if (text == _lastText && !IsForceUpdate)
                    return;

                //Update text record
                _lastText = text;

                //Display the hint
                var parameter = new HintParameter[] { new StringHintParameter(text) };
                var effect = new HintEffect[] { HintEffectPresets.TrailingPulseAlpha(1, 1, 1) };
                var hint = new TextHint(text, parameter, effect, float.MaxValue);

                ReferenceHub.connectionToClient.Send(new HintMessage(hint));
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
        /// <param name="referenceHub"></param>
        /// <returns></returns>
        public static PlayerDisplay Get(ReferenceHub referenceHub)
        {
            var pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return pd ?? new PlayerDisplay(referenceHub);//TryCreate ReferenceHub display if it has not been created yet
        }

        /// <summary>
        /// Exiled Only Method. Get the PlayerDisplay instance of the player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static PlayerDisplay Get(Player player)
        {
            return Get(player.ReferenceHub);
        }

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

            var toRemove = _hintList.Where(x => x.Id.Equals(id));

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
