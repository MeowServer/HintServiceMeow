using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Tests.Core.Utilities.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities
{
    [TestClass]
    public class PlayerDisplayTests
    {
        private TestTaskScheduler scheduler = null!;
        private TestCompatibilityAdaptor adaptor = null!;
        private TestHintParser parser = null!;
        private TestPlayerContext context = null!;
        private TestCoroutineRunner coroutineRunner = null!;
        private PlayerDisplay display = null!;

        [TestInitialize]
        public void SetUp()
        {
            scheduler = new TestTaskScheduler();
            adaptor = new TestCompatibilityAdaptor();
            parser = new TestHintParser();
            context = new TestPlayerContext { IsStillValid = false };
            coroutineRunner = new TestCoroutineRunner();

            display = new PlayerDisplay(context, updateScheduler: scheduler, adaptor: adaptor, hintParser: parser, coroutineRunner: coroutineRunner);
        }

        [TestCleanup]
        public void TearDown()
        {
            ((HintServiceMeow.Core.Interface.IDestructible)display).Destruct();
        }

        [TestMethod]
        public void Constructor_WhenPlayerContextNull_Throws()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => _ = new PlayerDisplay(null!));
        }

        [TestMethod]
        public void Constructor_WhenRunnerInjected_StartsCoroutine()
        {
            // Arrange - display already created in SetUp

            // Assert
            Assert.AreEqual(1, coroutineRunner.StartedRoutines.Count);
            Assert.IsTrue(coroutineRunner.LastCoroutine.IsRunning);
            Assert.IsFalse(coroutineRunner.LastCoroutine.IsKilled);
        }

        [TestMethod]
        public void Destruct_WhenCalled_KillsStartedCoroutine()
        {
            // Arrange - display already created in SetUp

            // Act
            ((HintServiceMeow.Core.Interface.IDestructible)display).Destruct();

            // Assert
            Assert.IsTrue(coroutineRunner.LastCoroutine.IsKilled);
            Assert.IsFalse(coroutineRunner.LastCoroutine.IsRunning);
        }

        [TestMethod]
        public void Destruct_WhenCalled_DestructsInjectedSchedulerAndAdaptor()
        {
            // Arrange - display already created in SetUp

            // Act
            ((HintServiceMeow.Core.Interface.IDestructible)display).Destruct();

            // Assert
            Assert.IsTrue(scheduler.IsDestructed);
            Assert.IsTrue(adaptor.IsDestructed);
        }

        [TestMethod]
        public void ForceUpdate_WhenCalled_SchedulesExpectedDelay()
        {
            // Arrange - display already created in SetUp

            // Act
            display.ForceUpdate();
            display.ForceUpdate(useFastUpdate: true);

            // Assert
            Assert.AreEqual(2, scheduler.Invokes.Count);
            Assert.IsTrue(scheduler.Invokes[0].Delay < 0.3f);
            Assert.IsTrue(scheduler.Invokes[1].Delay <= 0);
        }

        [TestMethod]
        public void AddAndRemoveHint_WhenManagingHints_ManagesCollectionByGuidAndId()
        {
            // Arrange
            Hint first = new() { Id = "hp" };
            Hint second = new() { Id = "hp" };

            // Act
            display.AddHint(first, second);

            // Assert - both hints added
            Assert.IsTrue(display.HasHint("hp"));
            Assert.IsTrue(display.HasHint(first.Guid));
            Assert.AreEqual(2, display.GetHints("hp").Count());

            // Act - remove by guid
            display.RemoveHint(first.Guid);

            // Assert - only first removed
            Assert.IsFalse(display.HasHint(first.Guid));
            Assert.IsTrue(display.HasHint(second.Guid));

            // Act - remove by id
            display.RemoveHint("hp");

            // Assert - all removed
            Assert.IsFalse(display.HasHint(second.Guid));
            Assert.AreEqual(0, display.GetHints().Count());
        }

        [TestMethod]
        public void TryGetHintAndClearHint_WhenUsed_ReturnsConsistentResult()
        {
            // Arrange
            Hint hint = new() { Id = "quest" };
            display.AddHint(hint);

            // Act
            bool foundByString = display.TryGetHint("quest", out AbstractHint stringHint);
            bool foundByGuid = display.TryGetHint(hint.Guid, out AbstractHint guidHint);
            display.ClearHint();

            // Assert
            Assert.IsTrue(foundByString);
            Assert.AreSame(hint, stringHint);
            Assert.IsTrue(foundByGuid);
            Assert.AreSame(hint, guidHint);
            Assert.IsFalse(display.TryGetHint("quest", out _));
            Assert.IsFalse(display.TryGetHint(hint.Guid, out _));
        }

        [TestMethod]
        public void AddHint_WhenInputIsNull_IsIgnored()
        {
            // Arrange - display already created in SetUp

            // Act
            display.AddHint((AbstractHint?)null);
            display.AddHint((AbstractHint[]?)null);
            display.AddHint(Array.Empty<AbstractHint>());
            display.AddHint((IEnumerable<AbstractHint>?)null);

            // Assert
            Assert.AreEqual(0, display.GetHints().Count());
        }

        [TestMethod]
        public void AddHint_WhenCollectionChanges_TriggersScheduleUpdate()
        {
            // Arrange
            int before = scheduler.Invokes.Count;

            // Act
            display.AddHint(new Hint { Id = "collection-changed" });

            // Assert
            Assert.IsTrue(scheduler.Invokes.Count > before);
            Assert.IsTrue(scheduler.Invokes.Last().Delay <= 0);
        }

        [TestMethod]
        public void HintPropertyUpdate_WhenSyncSpeedOrHideRulesApply_RespectsRules()
        {
            // Arrange - UnSync hint should not trigger scheduler
            Hint unSyncHint = new() { Id = "unsync", SyncSpeed = HintSyncSpeed.UnSync };
            display.AddHint(unSyncHint);
            int beforeUnSyncUpdate = scheduler.Invokes.Count;

            // Act
            unSyncHint.FontSize++;

            // Assert - unsync hint doesn't schedule
            Assert.AreEqual(beforeUnSyncUpdate, scheduler.Invokes.Count);

            // Arrange - hidden hint should not trigger scheduler
            Hint hiddenHint = new() { Id = "hidden", SyncSpeed = HintSyncSpeed.Fast, Hide = true };
            display.AddHint(hiddenHint);
            int beforeHiddenUpdate = scheduler.Invokes.Count;

            // Act
            hiddenHint.FontSize++;

            // Assert - hidden hint doesn't schedule
            Assert.AreEqual(beforeHiddenUpdate, scheduler.Invokes.Count);

            // Act - unhiding should schedule
            hiddenHint.Hide = false;

            // Assert
            Assert.IsTrue(scheduler.Invokes.Count > beforeHiddenUpdate);
            (float Delay, DelayType DelayType) lastInvoke = scheduler.Invokes.Last();
            Assert.IsTrue(lastInvoke.Delay <= 0.1f);
            Assert.AreEqual(DelayType.KeepFastest, lastInvoke.DelayType);
        }

        [TestMethod]
        [DataRow(HintSyncSpeed.Fastest, 0f)]
        [DataRow(HintSyncSpeed.Fast, 0.1f)]
        [DataRow(HintSyncSpeed.Normal, 0.3f)]
        [DataRow(HintSyncSpeed.Slow, 1f)]
        [DataRow(HintSyncSpeed.Slowest, 3f)]
        public void OnHintUpdate_WhenSyncSpeedVaries_MapsToExpectedScheduleDelay(HintSyncSpeed speed, float expectedMaxWait)
        {
            // Arrange
            Hint hint = new() { Id = "sync-map", SyncSpeed = speed };
            display.AddHint(hint);
            int before = scheduler.Invokes.Count;

            // Act
            hint.FontSize++;

            // Assert
            Assert.IsTrue(scheduler.Invokes.Count > before);
            (float Delay, DelayType delayType) invoke = scheduler.Invokes.Last();

            if (speed == HintSyncSpeed.Fastest)
            {
                Assert.IsTrue(invoke.Delay <= 0f);
                Assert.AreEqual(DelayType.Override, invoke.delayType);
            }
            else
            {
                Assert.AreEqual(DelayType.KeepFastest, invoke.delayType);
                Assert.IsTrue(invoke.Delay >= 0f);
                Assert.IsTrue(invoke.Delay <= expectedMaxWait + 0.05f);
            }
        }

        [TestMethod]
        public void ScheduleUpdate_WhenPredictionInWindow_UsesPredictedDelayAndKeepFastest()
        {
            // Arrange
            Hint updatingHint = new() { Id = "updating", SyncSpeed = HintSyncSpeed.Normal, UpdateAnalyser = new FixedUpdateAnalyser { NextUpdateTime = DateTime.Now.AddSeconds(10) } };
            Hint predictingHint = new() { Id = "predict", SyncSpeed = HintSyncSpeed.Fastest, UpdateAnalyser = new FixedUpdateAnalyser { NextUpdateTime = DateTime.Now.AddSeconds(0.25) } };
            display.AddHint(updatingHint, predictingHint);
            int before = scheduler.Invokes.Count;

            // Act
            updatingHint.FontSize++;

            // Assert
            Assert.IsTrue(scheduler.Invokes.Count > before);
            (float Delay, DelayType DelayType) invoke = scheduler.Invokes.Last();
            Assert.AreEqual(DelayType.KeepFastest, invoke.DelayType);
            Assert.IsTrue(invoke.Delay > 0.2f && invoke.Delay < 0.3f);
        }

        [TestMethod]
        public void ScheduleUpdate_WhenPredictionAfterWindow_ClampsToMaxWaitingTime()
        {
            // Arrange
            Hint updatingHint = new() { Id = "updating", SyncSpeed = HintSyncSpeed.Fast, UpdateAnalyser = new FixedUpdateAnalyser { NextUpdateTime = DateTime.Now.AddSeconds(10) } };
            Hint predictingHint = new() { Id = "predict", SyncSpeed = HintSyncSpeed.Slowest, UpdateAnalyser = new FixedUpdateAnalyser { NextUpdateTime = DateTime.Now.AddSeconds(5) } };
            display.AddHint(updatingHint, predictingHint);

            // Act
            updatingHint.Hide = true;

            // Assert
            (float Delay, DelayType DelayType) invoke = scheduler.Invokes.Last();
            Assert.AreEqual(DelayType.KeepFastest, invoke.DelayType);
            Assert.IsTrue(invoke.Delay >= 0f);
            Assert.IsTrue(invoke.Delay <= 0.02f);
        }

        [TestMethod]
        public void ScheduleUpdate_WhenHintHasDateTimeMaxValue_IsIgnored()
        {
            // Arrange
            Hint updatingHint = new() { Id = "updating", SyncSpeed = HintSyncSpeed.Slow, UpdateAnalyser = new FixedUpdateAnalyser { NextUpdateTime = DateTime.Now.AddSeconds(0.5) } };
            Hint ignoredMaxHint = new() { Id = "max", SyncSpeed = HintSyncSpeed.Slowest, UpdateAnalyser = new FixedUpdateAnalyser { NextUpdateTime = DateTime.MaxValue } };
            display.AddHint(updatingHint, ignoredMaxHint);

            // Act
            updatingHint.FontSize++;

            // Assert
            (float Delay, DelayType DelayType) invoke = scheduler.Invokes.Last();
            Assert.AreEqual(DelayType.KeepFastest, invoke.DelayType);
            Assert.IsTrue(invoke.Delay <= 0.02f);
        }

        [TestMethod]
        public void RemoveHint_WhenHintRemoved_PropertyChangesShouldNotSchedule()
        {
            // Arrange
            Hint hint = new() { Id = "remove", SyncSpeed = HintSyncSpeed.Normal };
            display.AddHint(hint);
            display.RemoveHint(hint);
            scheduler.Invokes.Clear();

            // Act
            hint.FontSize++;

            // Assert
            Assert.AreEqual(0, scheduler.Invokes.Count);
        }

        [TestMethod]
        public void ClearHint_WhenCleared_PropertyChangesShouldNotSchedule()
        {
            // Arrange
            Hint hint = new() { Id = "clear", SyncSpeed = HintSyncSpeed.Normal };
            display.AddHint(hint);
            display.ClearHint();
            scheduler.Invokes.Clear();

            // Act
            hint.FontSize++;

            // Assert
            Assert.AreEqual(0, scheduler.Invokes.Count);
        }

        [TestMethod]
        public void Destruct_WhenDestructed_PropertyChangesShouldNotScheduleOrThrow()
        {
            // Arrange
            Hint hint = new() { Id = "destruct", SyncSpeed = HintSyncSpeed.Normal };
            display.AddHint(hint);
            scheduler.Invokes.Clear();
            ((HintServiceMeow.Core.Interface.IDestructible)display).Destruct();

            // Act
            hint.FontSize++;

            // Assert
            Assert.AreEqual(0, scheduler.Invokes.Count);
        }

        [TestMethod]
        public void RemoveDisplayOutputOfType_WhenCalled_RemovesAllMatchedOutputs()
        {
            // Arrange
            TestDisplayOutput first = new();
            TestDisplayOutput second = new();
            display.AddDisplayOutput(first);
            display.AddDisplayOutput(second);

            // Act
            display.RemoveDisplayOutput<TestDisplayOutput>();
            InvokeSendHint(display, new DisplayOutputArg(display, "after-remove-type", Array.Empty<IParameter>(), Array.Empty<IEffect>(), 99999f));

            // Assert
            Assert.AreEqual(0, first.Calls.Count);
            Assert.AreEqual(0, second.Calls.Count);
        }

        [TestMethod]
        public void AddRemoveDisplayOutput_WhenSendHintCalled_DeliversToActiveOutputsOnly()
        {
            // Arrange
            TestDisplayOutput keepOutput = new();
            TestDisplayOutput removeOutput = new();
            TestDisplayOutput throwOutput = new() { ThrowOnShow = true };
            display.AddDisplayOutput(keepOutput);
            display.AddDisplayOutput(removeOutput);
            display.AddDisplayOutput(throwOutput);
            display.RemoveDisplayOutput(removeOutput);

            // Act
            InvokeSendHint(display, new DisplayOutputArg(display, "hello", Array.Empty<IParameter>(), Array.Empty<IEffect>(), 99999f));

            // Assert
            Assert.AreEqual(1, keepOutput.Calls.Count);
            Assert.AreEqual("hello", keepOutput.Calls[0].Content);
            Assert.AreEqual(0, removeOutput.Calls.Count);
        }

        [TestMethod]
        public void SchedulerCallback_WhenPipelineSuccess_PausesParseSendsAndResumes()
        {
            // Arrange
            TestMainThreadDispatcher dispatcher = new();
            TestDisplayOutput output = new();
            TestTaskScheduler localScheduler = new();
            PlayerDisplay localDisplay = new(
                new TestPlayerContext { IsStillValid = false },
                updateScheduler: localScheduler,
                adaptor: new TestCompatibilityAdaptor(),
                hintParser: new DelegateHintParser(_ => new HintParserResult("pipeline-success", Array.Empty<IParameter>())),
                coroutineRunner: new TestCoroutineRunner(),
                dispatcher: dispatcher,
                displayOutputs: new[] { output });

            try
            {
                // Act
                localScheduler.TriggerScheduledCallback();

                // Assert
                AssertEventually(() => output.Calls.Count == 1 && !localScheduler.IsPaused, 1000, "Parser pipeline did not complete in time");
                Assert.AreEqual("pipeline-success", output.Calls[0].Content);
                Assert.IsTrue(dispatcher.DispatchCallCount >= 1);
            }
            finally
            {
                ((HintServiceMeow.Core.Interface.IDestructible)localDisplay).Destruct();
            }
        }

        [TestMethod]
        public void SchedulerCallback_WhenParserThrows_ResumesWithoutSending()
        {
            // Arrange
            bool parserExecuted = false;
            TestMainThreadDispatcher dispatcher = new();
            TestDisplayOutput output = new();
            TestTaskScheduler localScheduler = new();
            PlayerDisplay localDisplay = new(
                new TestPlayerContext { IsStillValid = false },
                updateScheduler: localScheduler,
                adaptor: new TestCompatibilityAdaptor(),
                hintParser: new DelegateHintParser(_ =>
                {
                    parserExecuted = true;
                    throw new InvalidOperationException("parser failure");
                }),
                coroutineRunner: new TestCoroutineRunner(),
                dispatcher: dispatcher,
                displayOutputs: new[] { output });

            try
            {
                // Act
                localScheduler.TriggerScheduledCallback();

                // Assert
                AssertEventually(() => !localScheduler.IsPaused, 1000, "Scheduler should resume after parser exception");
                Assert.AreEqual(0, output.Calls.Count);
                Assert.IsTrue(parserExecuted);
            }
            finally
            {
                ((HintServiceMeow.Core.Interface.IDestructible)localDisplay).Destruct();
            }
        }

        [TestMethod]
        public void SchedulerCallback_WhenParserTaskAlreadyRunning_DoesNotStartParallelParserTask()
        {
            // Arrange
            TestTaskScheduler localScheduler = new();
            ManualResetEventSlim entered = new(false);
            ManualResetEventSlim release = new(false);
            DelegateHintParser blockingParser = new(_ =>
            {
                entered.Set();
                release.Wait(1000);
                return new HintParserResult("done", Array.Empty<IParameter>());
            });

            PlayerDisplay localDisplay = new(
                new TestPlayerContext { IsStillValid = false },
                updateScheduler: localScheduler,
                adaptor: new TestCompatibilityAdaptor(),
                hintParser: blockingParser,
                coroutineRunner: new TestCoroutineRunner(),
                dispatcher: new TestMainThreadDispatcher());

            try
            {
                // Act
                localScheduler.TriggerScheduledCallback();
                Assert.IsTrue(entered.Wait(500));
                Assert.IsTrue(localScheduler.IsPaused);

                localScheduler.TriggerScheduledCallback();
                Thread.Sleep(30);

                // Assert - parser called only once (second callback rejected)
                Assert.AreEqual(1, blockingParser.ParseCallCount);

                release.Set();
                AssertEventually(() => !localScheduler.IsPaused, 1000, "Scheduler should resume after blocking parser released");
            }
            finally
            {
                ((HintServiceMeow.Core.Interface.IDestructible)localDisplay).Destruct();
            }
        }

        [TestMethod]
        public void CoroutineMethod_WhenPlayerContextInvalid_Stops()
        {
            // Arrange
            IEnumerator<float> routine = coroutineRunner.StartedRoutines[0];

            // Act & Assert
            Assert.IsTrue(routine.MoveNext());
            Assert.AreEqual(-1f, routine.Current);
            Assert.IsFalse(routine.MoveNext());
        }

        [TestMethod]
        public void CoroutineMethod_WhenElapsedOverFiveSeconds_SchedulesPeriodicUpdate()
        {
            // Arrange
            context.IsStillValid = true;
            scheduler.Elapsed = TimeSpan.FromSeconds(6);
            IEnumerator<float> routine = coroutineRunner.StartedRoutines[0];

            // Act
            Assert.IsTrue(routine.MoveNext());
            Assert.AreEqual(-1f, routine.Current);
            Assert.IsTrue(routine.MoveNext());

            // Assert
            Assert.IsTrue(scheduler.Invokes.Any());
            Assert.IsTrue(scheduler.Invokes.Last().Delay <= 0);
        }

        [TestMethod]
        public void CoroutineMethod_WhenSchedulerReady_InvokesUpdateAvailable()
        {
            // Arrange
            context.IsStillValid = true;
            scheduler.IsReadyForNextAction = true;
            int callCount = 0;
            display.UpdateAvailable += _ => callCount++;
            IEnumerator<float> routine = coroutineRunner.StartedRoutines[0];

            // Act
            Assert.IsTrue(routine.MoveNext());
            Assert.IsTrue(routine.MoveNext());

            // Assert
            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void CoroutineMethod_WhenUpdateAvailableThrows_YieldsBackoffDelay()
        {
            // Arrange
            context.IsStillValid = true;
            scheduler.IsReadyForNextAction = true;
            display.UpdateAvailable += _ => throw new InvalidOperationException("update callback failed");
            IEnumerator<float> routine = coroutineRunner.StartedRoutines[0];

            // Act
            Assert.IsTrue(routine.MoveNext());
            Assert.AreEqual(-1f, routine.Current);

            Assert.IsTrue(routine.MoveNext());

            // Assert
            Assert.AreEqual(1f, routine.Current);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void RemoveHint_WhenStringGuardViolated_Throws(string? id)
        {
            // Arrange & Act & Assert
            if (id is null)
                Assert.ThrowsExactly<ArgumentNullException>(() => display.RemoveHint(id!));
            else
                Assert.ThrowsExactly<ArgumentException>(() => display.RemoveHint(id));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void StringQueryMethods_WhenStringGuardViolated_Throw(string? id)
        {
            // Arrange & Act & Assert
            if (id is null)
            {
                Assert.ThrowsExactly<ArgumentNullException>(() => display.GetHint(id));
                Assert.ThrowsExactly<ArgumentNullException>(() => display.GetHints(id!));
                Assert.ThrowsExactly<ArgumentNullException>(() => display.HasHint(id!));
                Assert.ThrowsExactly<ArgumentNullException>(() => display.TryGetHint(id!, out _));
                Assert.ThrowsExactly<ArgumentNullException>(() => display.TryGetHints(id, out _));
            }
            else
            {
                Assert.ThrowsExactly<ArgumentException>(() => display.GetHint(id));
                Assert.ThrowsExactly<ArgumentException>(() => display.GetHints(id));
                Assert.ThrowsExactly<ArgumentException>(() => display.HasHint(id));
                Assert.ThrowsExactly<ArgumentException>(() => display.TryGetHint(id, out _));
                Assert.ThrowsExactly<ArgumentException>(() => display.TryGetHints(id, out _));
            }
        }

        [TestMethod]
        public void HintParserSetter_AndCompatibilityAdaptorSetter_WhenSetToNull_Throws()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => display.HintParser = null!);
            Assert.ThrowsExactly<ArgumentNullException>(() => display.CompatibilityAdaptor = null!);
        }

        [TestMethod]
        public void ShowCompatibilityHint_WhenCalled_ForwardsToAdaptor()
        {
            // Arrange - display already created in SetUp

            // Act
            display.ShowCompatibilityHint("test-asm", "compat-content", 2.5f);

            // Assert
            Assert.AreEqual(1, adaptor.Calls.Count);
            Assert.AreEqual("test-asm", adaptor.Calls[0].AssemblyName);
            Assert.AreEqual("compat-content", adaptor.Calls[0].Content);
            Assert.AreEqual(2.5f, adaptor.Calls[0].Duration, 0.001f);
        }

        private static void AssertEventually(Func<bool> condition, int timeoutMs, string message)
        {
            DateTime end = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < end)
            {
                if (condition())
                    return;

                Thread.Sleep(10);
            }

            Assert.Fail(message);
        }

        private static void InvokeSendHint(PlayerDisplay pd, DisplayOutputArg arg)
        {
            MethodInfo method = typeof(PlayerDisplay).GetMethod("SendHint", BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(pd, [arg]);
        }
    }
}
