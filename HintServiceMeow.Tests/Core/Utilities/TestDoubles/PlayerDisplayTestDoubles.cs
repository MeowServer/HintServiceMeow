using System;
using System.Collections.Generic;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Arguments;

namespace HintServiceMeow.Tests.Core.Utilities.TestDoubles
{
    /// <summary>
    /// Minimal player context test double with controllable validity state.
    /// </summary>
    internal sealed class TestPlayerContext : IPlayerContext
    {
        public bool IsStillValid { get; set; }

        public bool IsValid() => IsStillValid;

        public bool Equals(IPlayerContext other)
        {
            return ReferenceEquals(this, other);
        }
    }

    /// <summary>
    /// Scheduler test double that records invoke requests for assertions.
    /// </summary>
    internal sealed class TestTaskScheduler : ITaskScheduler, HintServiceMeow.Core.Interface.IDestructible
    {
        private Func<bool> callback = null!;

        public bool InvokeUntilSuccess { get; set; }

        public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

        public TimeSpan MinInterval { get; set; } = TimeSpan.Zero;

        public bool IsReadyForNextAction { get; set; }

        public bool IsPaused { get; private set; }

        public bool IsDestructed { get; private set; }

        public List<(float Delay, DelayType DelayType)> Invokes { get; } = [];

        public void Start(TimeSpan interval)
        {
            Start(interval, () => { });
        }

        public void Start(TimeSpan interval, Action callback)
        {
            // Keep callback reference so tests can trigger it explicitly if needed.
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            this.callback = () =>
            {
                callback();
                return true;
            };
        }

        public void Start(TimeSpan interval, Func<bool> callback)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public void Invoke(float delay = -1f, DelayType delayType = DelayType.Override)
        {
            // Record all scheduling requests from PlayerDisplay.
            Invokes.Add((delay, delayType));
        }

        public void TriggerScheduledCallback()
        {
            // Simulate scheduler tick invoking the action registered in Start.
            callback();
        }

        public void Stop()
        {
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Destruct()
        {
            IsDestructed = true;
        }
    }

    /// <summary>
    /// Test coroutine implementation with controllable running state.
    /// </summary>
    internal sealed class TestCoroutine : ICoroutine
    {
        public bool IsRunning { get; private set; } = true;

        public bool IsPaused { get; private set; }

        public bool IsKilled { get; private set; }

        public void Kill()
        {
            IsKilled = true;
            IsRunning = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }
    }

    /// <summary>
    /// Coroutine runner test double that records started routines.
    /// </summary>
    internal sealed class TestCoroutineRunner : ICoroutineRunner
    {
        public List<IEnumerator<float>> StartedRoutines { get; } = [];

        public TestCoroutine LastCoroutine { get; private set; } = null!;

        public ICoroutine StartCoroutine(IEnumerator<float> routine)
        {
            StartedRoutines.Add(routine);
            LastCoroutine = new TestCoroutine();
            return LastCoroutine;
        }

        public ICoroutine CallAfter(TimeSpan time, Action action)
        {
            // For simplicity, treat CallAfter as a special case of StartCoroutine.
            IEnumerator<float> Routine()
            {
                yield return (float)time.TotalSeconds;
                action();
            }
            return StartCoroutine(Routine());
        }
    }

    /// <summary>
    /// Compatibility adaptor test double that records forwarding calls.
    /// </summary>
    internal sealed class TestCompatibilityAdaptor : ICompatibilityAdaptor, HintServiceMeow.Core.Interface.IDestructible
    {
        public List<CompatibilityAdaptorArg> Calls { get; } = [];

        public bool IsDestructed { get; private set; }

        public void ShowHint(CompatibilityAdaptorArg ev)
        {
            Calls.Add(ev);
        }

        public void Destruct()
        {
            IsDestructed = true;
        }
    }

    /// <summary>
    /// Hint parser test double that returns caller-provided text.
    /// </summary>
    internal sealed class TestHintParser : IHintParser
    {
        public HintParserResult ReturnResult { get; set; } = new HintParserResult("", Array.Empty<IHintParameter>());

        public int ParseCallCount { get; private set; }

        public HintParserResult ParseToMessage(HintCollection collection)
        {
            ParseCallCount++;
            return ReturnResult;
        }
    }

    internal sealed class DelegateHintParser : IHintParser
    {
        private readonly Func<HintCollection, HintParserResult> parseFunc;

        public DelegateHintParser(Func<HintCollection, HintParserResult> parseFunc)
        {
            this.parseFunc = parseFunc ?? throw new ArgumentNullException(nameof(parseFunc));
        }

        public int ParseCallCount { get; private set; }

        public HintParserResult ParseToMessage(HintCollection collection)
        {
            ParseCallCount++;
            return parseFunc(collection);
        }
    }

    internal sealed class TestMainThreadDispatcher : IMainThreadDispatcher
    {
        public int DispatchCallCount { get; private set; }

        public bool ThrowOnDispatch { get; set; }

        public void Dispatch(Action action)
        {
            DispatchCallCount++;

            if (ThrowOnDispatch)
                throw new InvalidOperationException("Dispatch failed in test");

            action();
        }
    }

    /// <summary>
    /// Display output test double that can optionally throw for resilience tests.
    /// </summary>
    internal sealed class TestDisplayOutput : IDisplayOutput
    {
        public List<DisplayOutputArg> Calls { get; } = [];

        public bool ThrowOnShow { get; set; }

        public void ShowHint(DisplayOutputArg ev)
        {
            if (ThrowOnShow)
                throw new InvalidOperationException("Display output test exception");

            Calls.Add(ev);
        }
    }

    /// <summary>
    /// Update analyzer test double with fixed prediction value.
    /// </summary>
    internal sealed class FixedUpdateAnalyser : IUpdateAnalyser
    {
        public DateTime NextUpdateTime { get; set; } = DateTime.MaxValue;

        public int OnUpdateCallCount { get; private set; }

        public void OnUpdate()
        {
            OnUpdateCallCount++;
        }

        public DateTime EstimateNextUpdate()
        {
            return NextUpdateTime;
        }
    }
}
