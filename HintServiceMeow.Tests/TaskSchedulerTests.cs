using HintServiceMeow.Core.Interface;
using HintServiceMeow.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

using TaskScheduler = HintServiceMeow.Core.Utilities.TaskScheduler;

namespace HintServiceMeow.Tests
{
    [TestClass]
    public class TaskSchedulerTests
    {
        private TaskScheduler _scheduler;
        private int _actionInvokeCount;

        [TestInitialize]
        public void SetUp()
        {
            _scheduler = new TaskScheduler(60); // tickRate=60 to ensure accuracy
            _actionInvokeCount = 0;
        }

        [TestCleanup]
        public void TearDown()
        {
            ((IDestructible)_scheduler).Destruct();
        }

        [TestMethod]
        public void Start_ZeroOrNegativeInterval_DoesNotThrow()
        {
            // Arrange & Act
            _scheduler.Start(TimeSpan.Zero, () => { });
            _scheduler.Start(TimeSpan.FromMilliseconds(-1), () => { });

            // Assert
            Assert.IsTrue(true); // No exception = passed
        }

        [TestMethod]
        public void Start_NullAction_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _scheduler.Start(TimeSpan.FromMilliseconds(100), null);
            });
        }

        [TestMethod]
        public void Start_ValidParameters_SetsIntervalAndAction()
        {
            // Arrange & Act
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () => { _actionInvokeCount++; });

            // Assert
            Assert.AreEqual(TimeSpan.Zero, _scheduler.Elapsed);
            Assert.IsFalse(_scheduler.IsPaused);
        }

        [TestMethod]
        public async Task Invoke_AfterInterval_ExecutesAction()
        {
            // Arrange
            var invoked = 0;
            _scheduler.Start(TimeSpan.FromMilliseconds(50), () => { invoked++; });

            // Act
            _scheduler.Invoke(0, Core.Enum.DelayType.Override);
            await Task.Delay(120);

            // Assert
            Assert.AreEqual(1, invoked);
        }

        [TestMethod]
        public void Invoke_KeepFastest_KeepsFasterScheduledTime()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { _actionInvokeCount++; });

            // Act
            _scheduler.Invoke(2f, Core.Enum.DelayType.KeepFastest);
            var firstTime = ReflectionHelper.GetPropertyValue<DateTime>(_scheduler, "ScheduledActionTime");
            _scheduler.Invoke(1f, Core.Enum.DelayType.KeepFastest);

            // Assert
            Assert.IsTrue(ReflectionHelper.GetPropertyValue<DateTime>(_scheduler, "ScheduledActionTime") <= firstTime);
        }

        [TestMethod]
        public void Invoke_KeepSlowest_KeepsSlowerScheduledTime()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { _actionInvokeCount++; });

            // Act
            _scheduler.Invoke(1f, Core.Enum.DelayType.KeepSlowest);
            var firstTime = ReflectionHelper.GetPropertyValue<DateTime>(_scheduler, "ScheduledActionTime");
            _scheduler.Invoke(10f, Core.Enum.DelayType.KeepSlowest);

            // Assert
            Assert.IsTrue(ReflectionHelper.GetPropertyValue<DateTime>(_scheduler, "ScheduledActionTime") >= firstTime);
        }

        [TestMethod]
        public void Invoke_Override_OverridesScheduledTime()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { _actionInvokeCount++; });

            // Act
            _scheduler.Invoke(3f, Core.Enum.DelayType.Override);
            _scheduler.Invoke(5f, Core.Enum.DelayType.Override);

            // Assert
            var scheduledTime = ReflectionHelper.GetPropertyValue<DateTime>(_scheduler, "ScheduledActionTime");
            Assert.IsTrue(Math.Abs((scheduledTime - DateTime.Now).TotalSeconds - 5f) < 0.2);
        }

        [TestMethod]
        public void Stop_WhenRunning_ResetsState()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { });
            _scheduler.Invoke(0.1f);

            // Act
            _scheduler.Stop();

            // Assert
            Assert.AreEqual(TimeSpan.Zero, _scheduler.Elapsed);
            Assert.AreEqual(DateTime.MaxValue,
                ReflectionHelper.GetPropertyValue<DateTime>(_scheduler, "ScheduledActionTime"));
        }

        [TestMethod]
        public void PauseAndResume_WhenStarted_PausesAndResumesCorrectly()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () => { });

            // Act - Pause
            _scheduler.Pause();

            // Assert - Paused
            Assert.IsTrue(_scheduler.IsPaused);
            var afterPause = _scheduler.Elapsed;
            Thread.Sleep(50);
            Assert.AreEqual(afterPause, _scheduler.Elapsed); // Elapsed time should not change while paused

            // Act - Resume
            _scheduler.Resume();

            // Assert - Resumed
            Assert.IsFalse(_scheduler.IsPaused);
        }

        [TestMethod]
        public void IsReadyForNextAction_ElapsedLessThanInterval_ReturnsFalse()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () =>
            {
                Console.WriteLine($"Action Executed at {DateTime.Now}");
            });

            // Act
            _scheduler.Invoke();
            Thread.Sleep(100 / 6);

            // Assert
            Assert.IsFalse(_scheduler.IsReadyForNextAction);
        }

        [TestMethod]
        public void IsReadyForNextAction_ElapsedGreaterThanInterval_ReturnsTrue()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(1), () =>
            {
                Console.WriteLine($"Action Executed at {DateTime.Now}");
            });

            // Act
            _scheduler.Invoke();
            Thread.Sleep(100 / 6); // Wait for 1 tick
            Thread.Sleep(10); // Wait for interval

            // Assert
            Console.WriteLine($"Elapsed for {_scheduler.Elapsed} since last action");
            Assert.IsTrue(_scheduler.IsReadyForNextAction);
        }

        [TestMethod]
        public void Elapsed_AfterInvoke_UpdatesCorrectly()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(1), () => { });
            _scheduler.Invoke(0);
            var oldElapsed = _scheduler.Elapsed;

            // Act
            Thread.Sleep(5);

            // Assert
            Assert.IsTrue(_scheduler.Elapsed >= oldElapsed);
        }
    }
}
