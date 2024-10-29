using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Tools;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Default implementation of UpdateAnalyser. Used to update hint's update
    /// </summary>
    internal class UpdateAnalyzer : IUpdateAnalyser
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly TimeSpan _leastInterval = TimeSpan.FromMilliseconds(50f);
        private readonly List<DateTime> _updateTimestamps = new List<DateTime>();

        private DateTime _cachedTime = DateTime.MaxValue;

        public void OnUpdate()
        {
            _lock.EnterWriteLock();

            try
            {
                var now = DateTime.Now;

                //Check if the Interval is too short
                if (!_updateTimestamps.IsEmpty() && now - _updateTimestamps.Last() < _leastInterval)
                    return;

                //Add timestamp and remove outdated ones
                _updateTimestamps.Add(now);
                _updateTimestamps.RemoveAll(x => now - x > TimeSpan.FromSeconds(30));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public DateTime EstimateNextUpdate()
        {
            _lock.EnterReadLock();

            try
            {
                if (_updateTimestamps.Count <= 1)
                {
                    return DateTime.MaxValue;
                }

                if (_cachedTime != DateTime.MaxValue)
                {
                    return _cachedTime;
                }

                long baseTicks = _updateTimestamps.First().Ticks;
                List<double> timeOffsets = _updateTimestamps.Select(date => (double)(date.Ticks - baseTicks)).ToList();
                List<int> xValues = Enumerable.Range(0, timeOffsets.Count).ToList();

                double avgX = xValues.Average();
                double avgY = timeOffsets.Average();

                double sumXY = xValues.Zip(timeOffsets, (x, y) => (x - avgX) * (y - avgY)).Sum();
                double sumXX = xValues.Sum(x => Math.Pow(x - avgX, 2));

                double slope = sumXY / sumXX;
                double intercept = avgY - slope * avgX;

                double nextOffset = slope * timeOffsets.Count + intercept;

                MultithreadTool.EnqueueAction(() => _cachedTime = DateTime.MaxValue);
                _cachedTime = new DateTime((long)(baseTicks + nextOffset));
                return _cachedTime;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return DateTime.MaxValue;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
