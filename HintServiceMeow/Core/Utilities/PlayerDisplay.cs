﻿using System;
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
using Log = PluginAPI.Core.Log;

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

        private static readonly Hints.TextHint HintTemplate = new Hints.TextHint("", new Hints.HintParameter[] { new Hints.StringHintParameter("") }, new Hints.HintEffect[] { Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1) }, float.MaxValue);

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

        private readonly TaskScheduler _taskScheduler = new TaskScheduler(TimeSpan.FromMilliseconds(50));

        #region Constructor and Destructors

        private PlayerDisplay(ReferenceHub referenceHub)
        {
            this.ConnectionToClient = referenceHub.netIdentity.connectionToClient;
            this.ReferenceHub = referenceHub;

            Timing.RunCoroutine(UpdateCoroutineMethod());
            Timing.RunCoroutine(CoroutineMethod());

            lock (PlayerDisplayListLock)
                PlayerDisplayList.Add(this);
        }

        internal static PlayerDisplay TryCreate(ReferenceHub referenceHub)
        {
             lock (PlayerDisplayListLock)
                return PlayerDisplayList.FirstOrDefault(x => x.ReferenceHub == referenceHub) ?? new PlayerDisplay(referenceHub);
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
                        if (_currentParserTask is null)
                            continue;

                        SendHint(_currentParserTask.Result);

                        _taskScheduler.ResetCountDown();

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
                    StartParserTask();
            }
        }

        #region Update Rate Management

        internal void OnHintUpdate(AbstractHint hint)
        {
            switch (hint.SyncSpeed)
            {
                case HintSyncSpeed.Fastest:
                    ScheduleUpdate();
                    break;
                case HintSyncSpeed.Fast:
                    ScheduleUpdate(hint, 0.1f);
                    break;
                case HintSyncSpeed.Normal:
                    ScheduleUpdate(hint, 0.3f);
                    break;
                case HintSyncSpeed.Slow:
                    ScheduleUpdate(hint, 1f);
                    break;
                case HintSyncSpeed.Slowest:
                    ScheduleUpdate(hint, 3f);
                    break;
                case HintSyncSpeed.UnSync:
                    break;
            }
        }

        private void ScheduleUpdate(AbstractHint updatingHint = null, float maxWaitingTime = 0)
        {
            lock(_currentParserTaskLock)
            {
                if (_currentParserTask != null)
                    return;
            }

            if (updatingHint is null || maxWaitingTime <= 0)
            {
                _taskScheduler.SetNextAction(StartParserTask);
                return;
            }

            var maxWaitingTimeSpan = TimeSpan.FromSeconds(maxWaitingTime);
            var now = DateTime.Now;

            DateTime newTime = _hints.AllHints
                .Where(h => h.SyncSpeed >= updatingHint.SyncSpeed && h != updatingHint)
                .Select(h => h.Analyser.EstimateNextUpdate())
                .Where(x => x - now >= TimeSpan.Zero && x - now <= maxWaitingTimeSpan)
                .DefaultIfEmpty(DateTime.Now)
                .Max();

            var delay = (float)(newTime - DateTime.Now).TotalSeconds;

            if(delay <= 0)
                _taskScheduler.SetNextAction(StartParserTask);
            else
                _taskScheduler.SetNextAction(StartParserTask, delay);
        }
        #endregion

        #region Update Methods

        /// <summary>
        /// Force an update when the update is available. You do not need to use this method unless you are using UnSync hints
        /// </summary>
        public void ForceUpdate(bool useFastUpdate = false)
        {
            ScheduleUpdate();
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

            ScheduleUpdate();
        }

        internal void InternalAddHint(string name, IEnumerable<AbstractHint> hints)
        {
            foreach (var hint in hints)
            {
                hint.HintUpdated += OnHintUpdate;
                UpdateAvailable += hint.TryUpdateHint;

                _hints.AddHint(name, hint);
            }

            ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, AbstractHint hint)
        {
            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, hint);

            ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, IEnumerable<AbstractHint> hints)
        {
            foreach (var hint in hints)
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;

                _hints.RemoveHint(name, hint);
            }

            ScheduleUpdate();
        }

        internal void InternalRemoveHint(string name, string id)
        {
            var hint = _hints.GetHints(name).FirstOrDefault(x => x.Id.Equals(id));

            if (hint == null)
                return;

            hint.HintUpdated -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            _hints.RemoveHint(name, x => x.Id.Equals(id));

            ScheduleUpdate();
        }

        internal void InternalClearHint(string name)
        {
            foreach(var hint in _hints.GetHints(name).ToList())
            {
                hint.HintUpdated -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            _hints.ClearHints(name);

            ScheduleUpdate();
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
