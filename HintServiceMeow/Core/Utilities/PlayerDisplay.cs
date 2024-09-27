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
using HintServiceMeow.Core.Utilities.Parser;

//Plugin API
using Log = PluginAPI.Core.Log;
using Exiled.API.Features;


namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Represent a player's display. This class is used to manage hints and update hint to player's display
    /// </summary>
    public class PlayerDisplay
    {
        /// <summary>
        /// Instance Trackers
        /// </summary>
        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = new HashSet<PlayerDisplay>();
        private static readonly object PlayerDisplayListLock = new object();

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

        private readonly HintParser _hintParser = new HintParser();

        private Task<string> _currentParserTask;
        private readonly object _currentParserTaskLock = new object();

        /// <summary>
        /// The text that was sent to the client in latest sync
        /// </summary>
        private string _lastText = string.Empty;

        private readonly UpdateScheduler _updateScheduler = new UpdateScheduler();
        private DateTime _arrangedUpdateTime = DateTime.MaxValue;
        private DateTime _planUpdateTime = DateTime.MaxValue;

        #region Update Rate Management

        internal void OnHintUpdate(AbstractHint hint)
        {
            switch (hint.SyncSpeed)
            {
                case HintSyncSpeed.Fastest:
                    _updateScheduler.UpdateWhenAvailable(true);
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

        private void UpdateWhenAvailable()
        {
            DateTime newUpdateTime = _updateScheduler.UpdateWhenAvailable();

            if (newUpdateTime < _planUpdateTime)
                _planUpdateTime = newUpdateTime;

            _planUpdateTime = _updateScheduler.UpdateWhenAvailable();
        }

        private void ArrangeUpdate(TimeSpan maxWaitingTime, AbstractHint hint)
        {
            DateTime newArrangeUpdateTime = _updateScheduler.ArrangeUpdate(maxWaitingTime, hint, _hints);

            if(newArrangeUpdateTime < _arrangedUpdateTime)
                _arrangedUpdateTime = newArrangeUpdateTime;
        }

        /// <summary>
        /// Include force update and UpdateAvailable event
        /// </summary>
        private static IEnumerator<float> CoroutineMethod()
        {
            while (true)
            {
                try
                {
                    lock (PlayerDisplayListLock)
                    {
                        foreach (PlayerDisplay pd in PlayerDisplayList)
                        {
                            //Force update
                            if (pd._updateScheduler.NeedPeriodicUpdate && pd._currentParserTask == null)
                                pd.StartParserTask();

                            //Invoke UpdateAvailable event
                            if (pd._updateScheduler.UpdateReady)
                                pd.UpdateAvailable?.Invoke(new UpdateAvailableEventArg(pd));
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

        /// <summary>
        /// Include update management
        /// </summary>
        private static IEnumerator<float> UpdateCoroutineMethod()
        {
            while (true)
            {
                try
                {
                    DateTime now = DateTime.Now;

                    lock (PlayerDisplayListLock)
                    {
                        foreach (PlayerDisplay pd in PlayerDisplayList)
                        {
                            //Check arranged update time.
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
                                pd.StartParserTask();
                            }

                            //If complete parsing, send the parsed text to client
                            if (pd._currentParserTask != null && pd._currentParserTask.IsCompleted)
                            {
                                var text = pd._currentParserTask.GetAwaiter().GetResult();

                                //Only update when text had changed since last update or if this is a force update
                                if (text != pd._lastText || pd._updateScheduler.NeedPeriodicUpdate)
                                {
                                    //Update text record
                                    pd._lastText = text;

                                    //Reset CountDown
                                    pd._updateScheduler.Reset();

                                    pd.SendHint(text);
                                }

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
            UpdateWhenAvailable();
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

            lock (PlayerDisplayListLock)
                PlayerDisplayList.Add(this);
        }

        static PlayerDisplay()
        {
            Timing.RunCoroutine(CoroutineMethod());
            Timing.RunCoroutine(UpdateCoroutineMethod());
        }

        internal static PlayerDisplay TryCreate(ReferenceHub referenceHub)
        {
            PlayerDisplay pd;

            lock (PlayerDisplayListLock)
                pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return pd ?? new PlayerDisplay(referenceHub);
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            lock (PlayerDisplayListLock)
                PlayerDisplayList.RemoveWhere(x => x.ReferenceHub == referenceHub);
        }

        internal static void ClearInstance()
        {
            lock (PlayerDisplayListLock)
                PlayerDisplayList.Clear();
        }

        #endregion

        #region Player Display Methods

        /// <summary>
        /// Get the PlayerDisplay instance of the player
        /// Not Thread Safe
        /// </summary>
        public static PlayerDisplay Get(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new Exception("A null ReferenceHub had been passed to Get method");

            PlayerDisplay pd;

            lock (PlayerDisplayListLock)
                pd = PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return pd ?? new PlayerDisplay(referenceHub);//TryCreate ReferenceHub display if it has not been created yet
        }

#if EXILED
        /// <summary>
        /// Get the PlayerDisplay instance of the player
        /// Not Thread Safe
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
            foreach(var hint in _hints.GetHints(name).ToList())
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
