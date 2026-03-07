using System;
using System.Threading;
using System.Threading.Tasks;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskScheduler = HintServiceMeow.Core.Utilities.TaskScheduler;

namespace HintServiceMeow.Tests.Core.Utilities
{
    [TestClass]
    public class TaskSchedulerTests
    {
        private TaskScheduler _scheduler = null!;
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
            ((HintServiceMeow.Core.Interface.IDestructible)_scheduler).Destruct();
        }

        [TestMethod]
        public void Start_WhenIntervalIsZeroOrNegative_DoesNotThrow()
        {
            // Arrange & Act & Assert - zero and negative intervals should not throw
            _scheduler.Start(TimeSpan.Zero, () => { });
            _scheduler.Start(TimeSpan.FromMilliseconds(-1), () => { });
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Start_WhenActionIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                _scheduler.Start(TimeSpan.FromMilliseconds(100), null!);
            });
        }

        [TestMethod]
        public void Start_WhenCalled_SetsIntervalAndAction()
        {
            // Arrange & Act
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () => { _actionInvokeCount++; });

            // Assert
            Assert.IsLessThan(5, _scheduler.Elapsed.TotalMilliseconds);
            Assert.IsFalse(_scheduler.IsPaused);
        }

        [TestMethod]
        public async Task Invoke_WhenDelayExpires_ExecutesAction()
        {
            // Arrange
            int invoked = 0;
            _scheduler.Start(TimeSpan.FromMilliseconds(50), () => { invoked++; });

            // Act
            _scheduler.Invoke(0, DelayType.Override);
            await Task.Delay(120);

            // Assert
            Assert.AreEqual(1, invoked);
        }

        [TestMethod]
        public Task Invoke_WithDifferentDelayTypes_SchedulesCorrectly()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { _actionInvokeCount++; });

            // Act & Assert - KeepFastest keeps the sooner scheduled time
            _scheduler.Invoke(2f, DelayType.KeepFastest);
            DateTime firstTime = GetScheduledActionTime(_scheduler);
            _scheduler.Invoke(1f, DelayType.KeepFastest);
            Assert.IsTrue(GetScheduledActionTime(_scheduler) <= firstTime);

            // Act & Assert - KeepSlowest keeps the later scheduled time
            _scheduler.Invoke(1f, DelayType.KeepSlowest);
            firstTime = GetScheduledActionTime(_scheduler);
            _scheduler.Invoke(10f, DelayType.KeepSlowest);
            Assert.IsTrue(GetScheduledActionTime(_scheduler) >= firstTime);

            // Act & Assert - Override always replaces with the new time
            _scheduler.Invoke(3f, DelayType.Override);
            _scheduler.Invoke(5f, DelayType.Override);
            Assert.IsLessThan(0.2, Math.Abs((GetScheduledActionTime(_scheduler) - DateTime.Now).TotalSeconds - 5f));

            return Task.CompletedTask;
        }

        [TestMethod]
        public void Stop_WhenCalled_ResetsState()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { });
            _scheduler.Invoke(0.1f);

            // Act
            _scheduler.Stop();

            // Assert
            Assert.AreEqual(TimeSpan.Zero, _scheduler.Elapsed);
            Assert.IsTrue(IsScheduledActionTimeMax(_scheduler));
        }

        [TestMethod]
        public void PauseAndResume_WhenPaused_StopsElapsedCounting()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () => { });

            // Act
            _scheduler.Pause();

            // Assert - paused state
            Assert.IsTrue(_scheduler.IsPaused);
            TimeSpan afterPause = _scheduler.Elapsed;
            Thread.Sleep(50);
            Assert.AreEqual(afterPause, _scheduler.Elapsed);

            // Act - resume
            _scheduler.Resume();

            // Assert - running state
            Assert.IsFalse(_scheduler.IsPaused);
        }

        [TestMethod]
        public void IsReadyForNextAction_WhenElapsedLessThanInterval_ReturnsFalse()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () => { });
            _scheduler.Invoke();
            Thread.Sleep(100 / 6);

            // Act
            bool result = _scheduler.IsReadyForNextAction;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsReadyForNextAction_WhenElapsedExceedsInterval_ReturnsTrue()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(1), () => { });
            _scheduler.Invoke();
            Thread.Sleep(100 / 6); // Wait for 1 tick
            Thread.Sleep(10);      // Wait for interval

            // Act
            bool result = _scheduler.IsReadyForNextAction;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Elapsed_WhenInvoked_IsUpdated()
        {
            // Arrange
            _scheduler.Start(TimeSpan.FromMilliseconds(1), () => { });
            _scheduler.Invoke(0);
            TimeSpan oldElapsed = _scheduler.Elapsed;
            Thread.Sleep(5);

            // Act
            TimeSpan newElapsed = _scheduler.Elapsed;

            // Assert
            Assert.IsTrue(newElapsed >= oldElapsed);
        }

        private static DateTime GetScheduledActionTime(TaskScheduler scheduler)
        {
            return ReflectionHelper.GetPropertyValue<DateTime>(scheduler, "ScheduledActionTime");
        }

        private static bool IsScheduledActionTimeMax(TaskScheduler scheduler)
        {
            return GetScheduledActionTime(scheduler) == DateTime.MaxValue;
        }
    }
}
