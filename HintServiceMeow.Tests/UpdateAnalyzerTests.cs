using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Tests.Helpers;
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
        public void EstimateNextUpdate_NoUpdates_ReturnsMaxValue()
        {
            // Arrange
            var analyzer = new UpdateAnalyzer();

            // Act
            var result = analyzer.EstimateNextUpdate();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, result, "Should be DateTime.MaxValue under initial condition");
        }

        [TestMethod]
        public void EstimateNextUpdate_SingleUpdate_ReturnsMaxValue()
        {
            // Arrange
            var analyzer = new UpdateAnalyzer();

            // Act
            analyzer.OnUpdate();
            var result = analyzer.EstimateNextUpdate();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, result, "Should be DateTime.MaxValue when having only 1 data");
        }

        [TestMethod]
        public void EstimateNextUpdate_TwoUpdates_ReturnsEstimatedTime()
        {
            // Arrange
            var analyzer = new UpdateAnalyzer();

            // Act
            analyzer.OnUpdate();
            Thread.Sleep(60);
            analyzer.OnUpdate();
            var next = analyzer.EstimateNextUpdate();

            // Assert
            Assert.AreNotEqual(DateTime.MaxValue, next, "Should return estimated time when having more than 1 data");
            Assert.IsTrue(next > DateTime.Now, "Estimated time should be later than current time.");
        }

        [TestMethod]
        public void OnUpdate_TooFrequentCall_IgnoresUpdate()
        {
            // Arrange
            var analyzer = new UpdateAnalyzer();
            analyzer.OnUpdate();
            Thread.Sleep(60);
            analyzer.OnUpdate(); // Valid call

            // Act
            var before = analyzer.EstimateNextUpdate();
            analyzer.OnUpdate(); // Invalid call due to short interval
            var after = analyzer.EstimateNextUpdate();

            // Assert
            Assert.AreEqual(before, after, "Analyzer should ignore the second call since the time elapsed between two action is too short");
        }

        [TestMethod]
        public void OnUpdate_WithOldTimestamps_RemovesExpiredEntries()
        {
            // Arrange
            var analyzer = new UpdateAnalyzer();
            var queue = ReflectionHelper.GetFieldValue<Queue<DateTime>>(analyzer, "_updateTimestamps");

            var old = DateTime.Now - TimeSpan.FromSeconds(31); // Old timestamp, should be removed during next OnUpdate call
            queue.Enqueue(old);
            queue.Enqueue(DateTime.Now);

            // Act
            Thread.Sleep(60);
            analyzer.OnUpdate(); // Should remove old timestamps here

            // Assert
            Assert.IsTrue(queue.Count <= 2, "Queue should remove timestamp that is older than 30 seconds");
        }

        [TestMethod]
        public void EstimateNextUpdate_CalledTwice_ReturnsCachedResult()
        {
            // Arrange
            var analyzer = new UpdateAnalyzer();
            analyzer.OnUpdate();
            Thread.Sleep(60);
            analyzer.OnUpdate();

            // Act
            var t1 = analyzer.EstimateNextUpdate();
            var t2 = analyzer.EstimateNextUpdate();

            // Assert
            Assert.AreEqual(t1, t2, "Two value should be identical due to cache");
        }

        [TestMethod]
        public void EstimateNextUpdate_NullTimestamps_ReturnsMaxValue()
        {
            // Arrange
            var analyzer = new UpdateAnalyzer();
            ReflectionHelper.SetFieldValue(analyzer, "_updateTimestamps", null); // use this to trigger NullReferenceException

            // Act
            var result = analyzer.EstimateNextUpdate();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, result, "Should return MaxValue when there's exception");
        }
    }
}
