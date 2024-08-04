using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Utilities
{
    internal class UpdateAnalyser
    {
        private DateTime _firstUpdate;

        private readonly TimeSpan _leastInterval = TimeSpan.FromMilliseconds(5f);

        //For update estimation
        private readonly List<DateTime> _updateTimestamps = new List<DateTime>();

        public void OnUpdate()
        {
            var now = DateTime.Now;

            if (_firstUpdate == default)
            {
                _firstUpdate = now;
                return;
            }
            
            if (now - _firstUpdate < _leastInterval)
                return;

            if (!_updateTimestamps.IsEmpty() && now - _updateTimestamps.Last() < _leastInterval)
                return;

            _updateTimestamps.Add(now);
            _updateTimestamps.RemoveAll(x => now - x > TimeSpan.FromSeconds(60));
        }

        public DateTime EstimateNextUpdate()
        {
            if (_updateTimestamps.Count < 2)
            {
                return DateTime.MaxValue;
            }
            
            var now = DateTime.Now;
            long nextTimestamp = long.MaxValue;

            _updateTimestamps.Add(now);

            try
            {
                //Calculate interval
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
            finally
            {
                _updateTimestamps.Remove(now);
            }

            return new DateTime(nextTimestamp);
        }
    }
}
