using HintServiceMeow.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HintServiceMeow.Tests
{
    [TestClass]
    public class UpdateAnalyzerTests
    {
        [TestMethod]
        public void EstimateNextUpdate_Initially_MaxValue()
        {
            var analyzer = new UpdateAnalyzer();
            Assert.AreEqual(DateTime.MaxValue, analyzer.EstimateNextUpdate(), "Should be DateTime.MaxValue under initial condition");
        }

        [TestMethod]
        public void EstimateNextUpdate_AfterFirstUpdate_StillMaxValue()
        {
            var analyzer = new UpdateAnalyzer();
            analyzer.OnUpdate();
            Assert.AreEqual(DateTime.MaxValue, analyzer.EstimateNextUpdate(), "Should be DateTime.MaxValue when having only 1 data");
        }

        [TestMethod]
        public void EstimateNextUpdate_AfterTwoUpdates_ShouldReturnTime()
        {
            var analyzer = new UpdateAnalyzer();

            analyzer.OnUpdate();
            Thread.Sleep(60);
            analyzer.OnUpdate();

            var next = analyzer.EstimateNextUpdate();
            Assert.AreNotEqual(DateTime.MaxValue, next, "Should return estimated time when having more than 1 data");
            Assert.IsTrue(next > DateTime.Now, "Estimated time should be later than current time.");
        }

        [TestMethod]
        public void OnUpdate_TooFrequent_ShouldIgnore()
        {
            var analyzer = new UpdateAnalyzer();

            analyzer.OnUpdate();
            Thread.Sleep(60);
            analyzer.OnUpdate(); // Valid call
            var before = analyzer.EstimateNextUpdate();
            analyzer.OnUpdate(); // Invalid call due to short interval
            var after = analyzer.EstimateNextUpdate();

            Assert.AreEqual(before, after, "Analyzer should ignore the second call since the time elapsed between two action is too short");
        }

        [TestMethod]
        public void OnUpdate_ShouldRemoveOldTimestamps()
        {
            var analyzer = new UpdateAnalyzer();

            // Inject data to simulate old timestamps
            var field = typeof(UpdateAnalyzer).GetField("_updateTimestamps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var queue = (Queue<DateTime>)field.GetValue(analyzer);

            var old = DateTime.Now - TimeSpan.FromSeconds(31); // Old timestamp, should be removed during next OnUpdate call
            queue.Enqueue(old);
            queue.Enqueue(DateTime.Now);

            Thread.Sleep(60);
            analyzer.OnUpdate();// Should remove old timestamps here

            Assert.IsTrue(queue.Count <= 2, "Queue should remove timestamp that is older than 30 seconds");
        }

        [TestMethod]
        public void EstimateNextUpdate_CachesResult()
        {
            var analyzer = new UpdateAnalyzer();
            analyzer.OnUpdate();
            Thread.Sleep(60);
            analyzer.OnUpdate();

            var t1 = analyzer.EstimateNextUpdate();
            var t2 = analyzer.EstimateNextUpdate();
            Assert.AreEqual(t1, t2, "Two value should be identical due to cache");
        }

        [TestMethod]
        public void EstimateNextUpdate_Exception_ShouldReturnMaxValue()
        {
            var analyzer = new UpdateAnalyzer();
            var field = typeof(UpdateAnalyzer).GetField("_updateTimestamps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(analyzer, null); // use this to trigger NullReferenceException

            Assert.AreEqual(DateTime.MaxValue, analyzer.EstimateNextUpdate(), "Should return MaxValue when there's exception");
        }
    }
}
