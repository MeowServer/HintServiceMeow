using System;
using System.Collections.Generic;
using System.Linq;
using HintServiceMeow.Core.Interface;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Used to estimate the next update time.
    /// </summary>
    internal class UpdateAnalyser : IUpdateAnalyser
    {
        private readonly object _lock = new object();

        private readonly TimeSpan _leastInterval = TimeSpan.FromMilliseconds(50f);

        private readonly List<DateTime> _updateTimestamps = new List<DateTime>();

        public void OnUpdate()
        {
            lock (_lock)
            {
                var now = DateTime.Now;

                //Check if the Interval is too short
                if (!_updateTimestamps.IsEmpty() && now - _updateTimestamps.Last() < _leastInterval)
                    return;

                //Add timestamp and remove outdated ones
                _updateTimestamps.Add(now);
                _updateTimestamps.RemoveAll(x => now - x > TimeSpan.FromSeconds(30));
            }
        }

        public DateTime EstimateNextUpdate()
        {
            lock (_lock)
            {
                if (_updateTimestamps.Count <= 1)
                {
                    return DateTime.MaxValue;
                }

                var now = DateTime.Now;
                long nextTimestamp = long.MaxValue;

                _updateTimestamps.Add(now);

                try
                {
                    //Calculate Interval
                    List<long> intervals = new List<long>();
                    for (int i = 1; i < _updateTimestamps.Count; i++)
                    {
                        intervals.Add(_updateTimestamps[i].Ticks - _updateTimestamps[i - 1].Ticks);
                    }

                    //Prepare data
                    long[] xData = new long[intervals.Count];
                    long[] yData = intervals.ToArray();
                    for (int i = 0; i < xData.Length; i++)
                    {
                        xData[i] = i;
                    }

                    // Linear regression
                    int n = xData.Length;
                    float sumX = xData.Sum();
                    float sumY = yData.Sum();
                    float sumXY = 0;
                    float sumX2 = 0;

                    for (int i = 0; i < n; i++)
                    {
                        sumXY += xData[i] * yData[i];
                        sumX2 += xData[i] * xData[i];
                    }

                    float slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
                    float intercept = (sumY - slope * sumX) / n;

                    float nextInterval = slope * n + intercept;

                    nextTimestamp = _updateTimestamps.Last().Ticks + (long)nextInterval;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                _updateTimestamps.Remove(now);

                return new DateTime(nextTimestamp);
            }
        }
    }
}
