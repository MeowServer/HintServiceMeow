using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core;
using static HintServiceMeow.Core.Utilities.PlayerDisplay;
using HintServiceMeow.Core.Models;

namespace HintServiceMeow.Core.Utilities
{
    internal class UpdateScheduler
    {
        private readonly object _rateDataLock = new object();

        /// <summary>
        /// The time of latest update
        /// </summary>
        private DateTime _lastTimeUpdate = DateTime.MinValue;

        /// <summary>
        /// The time of second-last update
        /// </summary>
        private DateTime _secondLastTimeUpdate = DateTime.MinValue;

        /// <summary>
        /// The minimum interval between a regular update and the last update
        /// </summary>
        public static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5f);

        /// <summary>
        /// The minimum interval between a force update and the second last update
        /// </summary>
        public static readonly TimeSpan SecondLastUpdateInterval = TimeSpan.FromSeconds(1f);

        /// <summary>
        /// The player display will be forced to update when there haven't been any update in this amount of time
        /// </summary>
        public static readonly TimeSpan PeriodicUpdateInterval = TimeSpan.FromSeconds(5f);

        /// <summary>
        /// The time left for regular update to cool down.
        /// </summary>
        public TimeSpan UpdateCoolDown
        {
            get
            {
                TimeSpan delay1 = (this._lastTimeUpdate + UpdateInterval - DateTime.Now);
                TimeSpan delay2 = (this._secondLastTimeUpdate + SecondLastUpdateInterval - DateTime.Now);
                return delay1 > delay2 ? delay1 : delay2;
            }
        }

        /// <summary>
        /// Whether regular update is ready
        /// </summary>
        public bool UpdateReady => UpdateCoolDown.Ticks <= 0;

        /// <summary>
        /// The time left for fast update to cool down.
        /// </summary>
        public TimeSpan FastUpdateCoolDown => this._secondLastTimeUpdate + SecondLastUpdateInterval - DateTime.Now;

        /// <summary>
        /// Whether fast update is ready
        /// </summary>
        public bool FastUpdateReady => FastUpdateCoolDown.Ticks <= 0;

        /// <summary>
        /// The time PlayerDisplay will execute a periodic update
        /// </summary>
        public TimeSpan PeriodicUpdateCoolDown => _lastTimeUpdate + PeriodicUpdateInterval - DateTime.Now;

        /// <summary>
        /// Whether PlayerDisplay need to force update or not
        /// </summary>
        public bool NeedPeriodicUpdate => PeriodicUpdateCoolDown.Ticks <= 0;

        /// <summary>
        /// Return the next time when an update is available
        /// </summary>
        public DateTime UpdateWhenAvailable(bool useFastUpdate = false)
        {
            lock (_rateDataLock)
            {
                TimeSpan timeToWait = useFastUpdate ? FastUpdateCoolDown : UpdateCoolDown;

                return DateTime.Now + timeToWait + TimeSpan.FromMilliseconds(5);
            }
        }

        /// <summary>
        /// Return the next time when an update should be arranged
        /// </summary>
        public DateTime ArrangeUpdate(TimeSpan maxDelay, AbstractHint hint, HintCollection hints)
        {
            try
            {
                var now = DateTime.Now;

                //Find the latest estimated update time within maxDelay
                return hints.AllHints
                    .Where(h => h.SyncSpeed >= hint.SyncSpeed && h != hint)
                    .Select(h => h.Analyser.EstimateNextUpdate())
                    .Where(x => x - now >= TimeSpan.Zero && x - now <= maxDelay)
                    .DefaultIfEmpty(DateTime.Now)
                    .Max();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return DateTime.Now;
        }

        public void Reset()
        {
            _secondLastTimeUpdate = _lastTimeUpdate;
            _lastTimeUpdate = DateTime.Now;
        }
    }
}
