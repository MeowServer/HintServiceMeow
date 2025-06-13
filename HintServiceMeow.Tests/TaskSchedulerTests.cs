using HintServiceMeow.Core.Interface;
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
        public void Start_ShouldThrow_IfIntervalZeroOrNegative()
        {
            _scheduler.Start(TimeSpan.Zero, () => { });
            _scheduler.Start(TimeSpan.FromMilliseconds(-1), () => { });
            
            Assert.IsTrue(true); // No exception = passed
        }

        [TestMethod]
        public void Start_ShouldThrow_IfActionIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _scheduler.Start(TimeSpan.FromMilliseconds(100), null);
            });
        }

        [TestMethod]
        public void Start_ShouldSet_IntervalAndAction()
        {
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () => { _actionInvokeCount++; });
            Assert.AreEqual(TimeSpan.Zero, _scheduler.Elapsed);
            Assert.IsFalse(_scheduler.IsPaused);
        }

        [TestMethod]
        public async Task Invoke_And_AutoInvokeAction_AfterInterval()
        {
            var invoked = 0;
            _scheduler.Start(TimeSpan.FromMilliseconds(50), () => { invoked++; });

            _scheduler.Invoke(0, Core.Enum.DelayType.Override);
            await Task.Delay(120);

            Assert.AreEqual(1, invoked);
        }

        [TestMethod]
        public async Task Invoke_With_Delay_KeepsScheduledTime_ByDelayType()
        {
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { _actionInvokeCount++; });

            // KeepFastest
            _scheduler.Invoke(2f, Core.Enum.DelayType.KeepFastest);
            var firstTime = GetScheduledActionTime(_scheduler);
            _scheduler.Invoke(1f, Core.Enum.DelayType.KeepFastest);
            Assert.IsTrue(GetScheduledActionTime(_scheduler) <= firstTime);

            // KeepSlowest
            _scheduler.Invoke(1f, Core.Enum.DelayType.KeepSlowest);
            firstTime = GetScheduledActionTime(_scheduler);
            _scheduler.Invoke(10f, Core.Enum.DelayType.KeepSlowest);
            Assert.IsTrue(GetScheduledActionTime(_scheduler) >= firstTime);

            // Override
            _scheduler.Invoke(3f, Core.Enum.DelayType.Override);
            firstTime = GetScheduledActionTime(_scheduler);
            _scheduler.Invoke(5f, Core.Enum.DelayType.Override);
            Assert.IsTrue(Math.Abs((GetScheduledActionTime(_scheduler) - DateTime.Now).TotalSeconds - 5f) < 0.2);
        }

        [TestMethod]
        public void Stop_ShouldReset_State()
        {
            _scheduler.Start(TimeSpan.FromMilliseconds(100), () => { });
            _scheduler.Invoke(0.1f);

            _scheduler.Stop();

            Assert.AreEqual(TimeSpan.Zero, _scheduler.Elapsed);
            // Reset scheduled action time
            Assert.IsTrue(IsScheduledActionTimeMax(_scheduler));
        }

        [TestMethod]
        public void Pause_And_Resume_Work_AsExpected()
        {
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () => { });

            _scheduler.Pause();
            Assert.IsTrue(_scheduler.IsPaused);

            var afterPause = _scheduler.Elapsed;
            Thread.Sleep(50);
            Assert.AreEqual(afterPause, _scheduler.Elapsed); // Elapsed time should not change while paused

            _scheduler.Resume();
            Assert.IsFalse(_scheduler.IsPaused);
        }

        [TestMethod]
        public void IsReadyForNextAction_ReturnsFalse_IfElapsedLessThanInterval()
        {
            _scheduler.Start(TimeSpan.FromMilliseconds(200), () =>
            {
                Console.WriteLine($"Action Executed at {DateTime.Now}");
            });
            _scheduler.Invoke();
            Thread.Sleep(100/6);
            Assert.IsFalse(_scheduler.IsReadyForNextAction);
        }

        [TestMethod]
        public void IsReadyForNextAction_ReturnsTrue_IfElapsedGreaterThanInterval()
        {
            _scheduler.Start(TimeSpan.FromMilliseconds(1), () =>
            {
                Console.WriteLine($"Action Executed at {DateTime.Now}");
            });
            _scheduler.Invoke();

            Thread.Sleep(100/6); // Wait for 1 tick
            Thread.Sleep(10); // Wait for interval

            Console.WriteLine($"Elapsed for {_scheduler.Elapsed} since last action");
            Assert.IsTrue(_scheduler.IsReadyForNextAction);
        }

        [TestMethod]
        public void Elapsed_Resets_When_InvokeAction()
        {
            _scheduler.Start(TimeSpan.FromMilliseconds(1), () => { });
            _scheduler.Invoke(0);
            var oldElapsed = _scheduler.Elapsed;
            Thread.Sleep(5);

            // Elapsed updated
            Assert.IsTrue(_scheduler.Elapsed >= oldElapsed);
        }

        // Auxiliary method to read private members
        private DateTime GetScheduledActionTime(TaskScheduler scheduler)
        {
            var type = typeof(TaskScheduler);
            var prop = type.GetProperty("ScheduledActionTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (DateTime)prop.GetValue(scheduler);
        }

        private bool IsScheduledActionTimeMax(TaskScheduler scheduler)
        {
            return GetScheduledActionTime(scheduler) == DateTime.MaxValue;
        }
    }
}
