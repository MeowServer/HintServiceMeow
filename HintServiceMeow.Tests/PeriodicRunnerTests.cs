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
        public async Task Start_RunsPeriodically()
        {
            int count = 0;

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
                Assert.IsTrue(count >= 4);
            }
        }

        [TestMethod]
        public async Task Start_WithRunImmediately_InvokesAtOnce()
        {
            int count = 0;

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
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        public async Task PauseAndResume_Works()
        {
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
                int beforePause = count;

                runner.Pause();
                await Task.Delay(GetLength(ShortInterval, 4));
                Assert.AreEqual(beforePause, count);

                runner.Resume();
                await Task.Delay(GetLength(ShortInterval, 3));
                Assert.IsTrue(count > beforePause);
            }
        }

        [TestMethod]
        public async Task Dispose_StopsFurtherInvocations()
        {
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

            runner.Dispose();
            await Task.Delay(GetLength(ShortInterval, 4));
            Assert.AreEqual(beforeDispose, count);

            await runner.CurrentTask;
        }

        [TestMethod]
        public async Task Callback_Exception_IsSwallowedAndContinues()
        {
            int count = 0;

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
                Assert.IsTrue(count >= 3);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NegativeInterval_Throws()
        {
            PeriodicRunner.Start(() => Task.CompletedTask,
                                 TimeSpan.FromMilliseconds(-1));
        }
    }
}
