using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Tools;
using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Default implementation of UpdateAnalyser. Used to update hint's update
    /// </summary>
    internal class UpdateAnalyzer : IUpdateAnalyser
    {
        private readonly object _lock = new();

        private static readonly TimeSpan LeastInterval = TimeSpan.FromMilliseconds(50);
        private static readonly TimeSpan WindowInterval = TimeSpan.FromSeconds(30);

        private DateTime _lastUpdateTime = DateTime.MinValue; // Last time the update was called
        private readonly Queue<DateTime> _updateTimestamps = new(100);

        private DateTime _cachedTime = DateTime.MaxValue;

        public void OnUpdate()
        {
            lock (_lock)
            {
                DateTime now = DateTime.Now;

                //Check if the Interval is too short
                if (now - _lastUpdateTime < LeastInterval)
                    return;

                //Add timestamp and remove outdated ones
                _lastUpdateTime = now;
                _updateTimestamps.Enqueue(now);
                while (now - _updateTimestamps.Peek() > WindowInterval)
                {
                    _updateTimestamps.Dequeue();
                }

                _cachedTime = DateTime.MaxValue; // Reset cached time
            }
        }

        public DateTime EstimateNextUpdate()
        {
            lock (_lock)
            {
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

                    long baseT = _updateTimestamps.Peek().Ticks;
                    int n = _updateTimestamps.Count;
                    double sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
                    int i = 0;

                    foreach (DateTime dt in _updateTimestamps)
                    {
                        double x = i++;
                        double y = dt.Ticks - baseT;
                        sumX += x;
                        sumY += y;
                        sumXY += x * y;
                        sumXX += x * x;
                    }

                    double meanX = sumX / n;
                    double meanY = sumY / n;

                    double w1 = (sumXY - n * meanX * meanY) / (sumXX - n * meanX * meanX);
                    double w0 = meanY - w1 * meanX;

                    _cachedTime = new DateTime((long)(baseT + w1 * (n + 1) + w0));
                    return _cachedTime;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                    return DateTime.MaxValue;
                }
            }
        }
    }
}
