using HintServiceMeow.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HintServiceMeow.Tests
{
    [TestClass]
    public class PeriodicRunnerTests
    {
        private static readonly TimeSpan ShortInterval = TimeSpan.FromMilliseconds(30);

        private static TimeSpan GetLength(TimeSpan interval, int times)
        {
            return TimeSpan.FromTicks((long)(interval.Ticks * times));
        }

        [TestMethod]
        public async Task Start_WithInterval_RunsPeriodically()
        {
            // Arrange
            int count = 0;

            // Act
            using (var runner = PeriodicRunner.Start(
                       () =>
                       {
                           Interlocked.Increment(ref count);
                           return Task.CompletedTask;
                       },
                       ShortInterval,
                       runImmediately: false))
            {
                await Task.Delay(GetLength(ShortInterval, 5));

                // Assert
                Assert.IsTrue(count >= 4);
            }
        }

        [TestMethod]
        public async Task Start_WithRunImmediately_InvokesAtOnce()
        {
            // Arrange
            int count = 0;

            // Act
            using (var runner = PeriodicRunner.Start(
                       () =>
                       {
                           Interlocked.Increment(ref count);
                           return Task.CompletedTask;
                       },
                       ShortInterval,
                       runImmediately: true))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                // Assert
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        public async Task PauseAndResume_WhenRunning_StopsAndResumesExecution()
        {
            // Arrange
            int count = 0;

            using (var runner = PeriodicRunner.Start(
                       () =>
                       {
                           Interlocked.Increment(ref count);
                           return Task.CompletedTask;
                       },
                       ShortInterval))
            {
                await Task.Delay(GetLength(ShortInterval, 3));

                // Act - Pause
                runner.Pause();
                int before = count;
                await Task.Delay(GetLength(ShortInterval, 4));

                // Assert - Paused
                Assert.AreEqual(before, count);

                // Act - Resume
                runner.Resume();
                await Task.Delay(GetLength(ShortInterval, 3));

                // Assert - Resumed
                Assert.IsTrue(count > before);
            }
        }

        [TestMethod]
        public async Task Dispose_WhenRunning_StopsFurtherInvocations()
        {
            // Arrange
            int count = 0;
            var runner = PeriodicRunner.Start(
                () =>
                {
                    Interlocked.Increment(ref count);
                    return Task.CompletedTask;
                },
                ShortInterval);

            await Task.Delay(GetLength(ShortInterval, 3));
            int beforeDispose = count;

            // Act
            runner.Dispose();
            await Task.Delay(GetLength(ShortInterval, 4));

            // Assert
            Assert.AreEqual(beforeDispose, count);

            await runner.CurrentTask;
        }

        [TestMethod]
        public async Task Start_CallbackThrowsException_ContinuesExecution()
        {
            // Arrange
            int count = 0;

            // Act
            using (var runner = PeriodicRunner.Start(
                       () =>
                       {
                           int cur = Interlocked.Increment(ref count);
                           if (cur == 1)
                               throw new InvalidOperationException("Test");
                           return Task.CompletedTask;
                       },
                       ShortInterval))
            {
                await Task.Delay(GetLength(ShortInterval, 4));

                // Assert
                Assert.IsTrue(count >= 3);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Start_NegativeInterval_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            PeriodicRunner.Start(() => Task.CompletedTask,
                                 TimeSpan.FromMilliseconds(-1));

            // Assert - ExpectedException
        }
    }
}
