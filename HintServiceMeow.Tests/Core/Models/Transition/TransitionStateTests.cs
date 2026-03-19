namespace HintServiceMeow.Tests.Core.Models.Transition
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Transition;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // ────────────────────────────────────────────────────────────────
    //  TransitionState Tests
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tests for <see cref="TransitionState"/>.
    /// Because TransitionState reads <c>NetworkTimeCache.Time</c> directly,
    /// we need to control that value. The helper below uses reflection to
    /// set the backing field. Adjust if your project exposes a test seam.
    /// </summary>
    [TestClass]
    public class TransitionStateTests
    {
        private FakeCurveFactory factory = null!;

        [TestInitialize]
        public void Setup()
        {
            factory = new FakeCurveFactory();
            Transition.CurveFactory = factory;

            // Set NetworkTimeCache.Time to a known baseline
            SetNetworkTime(100.0);
        }

        // ---------- Constructor ----------

        [TestMethod]
        public void Ctor_StoresFromAndToValues()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            var state = new TransitionState(t, fromValue: 10f, toValue: 90f);

            Assert.AreEqual(10f, state.FromValue);
            Assert.AreEqual(90f, state.ToValue);
        }

        [TestMethod]
        public void Ctor_StoresTransitionReference()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 5f);
            var state = new TransitionState(t, 0f, 1f);

            Assert.AreSame(t, state.Transition);
        }

        [TestMethod]
        public void Ctor_CapturesStartTimeFromNetworkTimeCache()
        {
            SetNetworkTime(42.5);
            var t = Transition.Get(EasingType.EaseInOut, duration: 1f);
            var state = new TransitionState(t, 0f, 1f);

            Assert.AreEqual(42.5, state.StartTime, 0.001);
        }

        // ---------- Duration ----------

        [TestMethod]
        public void Duration_DelegatesToTransition()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 7f);
            var state = new TransitionState(t, 0f, 1f);

            Assert.AreEqual(7f, state.Duration);

            t.Duration = 3f;
            Assert.AreEqual(3f, state.Duration);
        }

        // ---------- IsExpired ----------

        [TestMethod]
        public void IsExpired_JustCreated_ReturnsFalse()
        {
            SetNetworkTime(100.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 5f);
            var state = new TransitionState(t, 0f, 1f);

            // Still at time 100 → elapsed = 0
            Assert.IsFalse(state.IsExpired);
        }

        [TestMethod]
        public void IsExpired_ElapsedEqualsDuration_ReturnsTrue()
        {
            SetNetworkTime(100.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 3f);
            var state = new TransitionState(t, 0f, 1f);

            SetNetworkTime(103.0); // elapsed = 3 == duration
            Assert.IsTrue(state.IsExpired);
        }

        [TestMethod]
        public void IsExpired_ElapsedExceedsDuration_ReturnsTrue()
        {
            SetNetworkTime(100.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            var state = new TransitionState(t, 0f, 1f);

            SetNetworkTime(110.0);
            Assert.IsTrue(state.IsExpired);
        }

        [TestMethod]
        public void IsExpired_ElapsedLessThanDuration_ReturnsFalse()
        {
            SetNetworkTime(100.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 5f);
            var state = new TransitionState(t, 0f, 1f);

            SetNetworkTime(102.0); // elapsed = 2 < 5
            Assert.IsFalse(state.IsExpired);
        }

        // ---------- CurrentValue ----------

        [TestMethod]
        public void CurrentValue_WhenExpired_ReturnsToValue()
        {
            SetNetworkTime(100.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            var state = new TransitionState(t, 10f, 80f);

            SetNetworkTime(103.0); // well past duration
            Assert.AreEqual(80f, state.CurrentValue, 0.001f);
        }

        [TestMethod]
        public void CurrentValue_AtHalfDuration_ReturnsInterpolatedValue()
        {
            SetNetworkTime(100.0);
            // Linear FakeCurve: Evaluate(t) = t
            var t = Transition.Get(EasingType.EaseInOut, duration: 4f);
            var state = new TransitionState(t, 0f, 100f);

            SetNetworkTime(102.0); // elapsed=2, half of duration=4
            // Evaluate(2, 0, 100) → linear → 50
            Assert.AreEqual(50f, state.CurrentValue, 0.1f);
        }

        [TestMethod]
        public void CurrentValue_AtStart_ReturnsFromValue()
        {
            SetNetworkTime(100.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 3f);
            var state = new TransitionState(t, 20f, 80f);

            // Still at start time → elapsed=0 → Evaluate(0,20,80) = 20
            Assert.AreEqual(20f, state.CurrentValue, 0.001f);
        }

        [TestMethod]
        public void CurrentValue_ExactlyAtDuration_ReturnsToValue()
        {
            SetNetworkTime(100.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 5f);
            var state = new TransitionState(t, 0f, 50f);

            SetNetworkTime(105.0); // elapsed == duration → IsExpired path → ToValue
            Assert.AreEqual(50f, state.CurrentValue, 0.001f);
        }

        [TestMethod]
        public void CurrentValue_ReversedRange_Interpolates()
        {
            SetNetworkTime(0.0);
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            var state = new TransitionState(t, 100f, 0f);

            SetNetworkTime(1.0); // elapsed=1, half duration
            // Evaluate(1, 100, 0) → linear 0.5 → 100 + (-100*0.5) = 50
            Assert.AreEqual(50f, state.CurrentValue, 0.1f);
        }

        // ---------- Helper ----------

        /// <summary>
        /// Sets <c>NetworkTimeCache.Time</c> via reflection.
        /// Adjust this to match your actual implementation (static property / field).
        /// </summary>
        private static void SetNetworkTime(double value)
        {
            var type = typeof(NetworkTimeCache);

            // Try a static property setter first
            var prop = type.GetProperty("Time",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static);

            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(null, value);
                return;
            }

            // Fall back to backing field (_time, time, etc.)
            foreach (var name in new[] { "_time", "time", "<Time>k__BackingField" })
            {
                var field = type.GetField(name,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static);

                if (field != null)
                {
                    field.SetValue(null, value);
                    return;
                }
            }

            Assert.Inconclusive(
                "Could not set NetworkTimeCache.Time via reflection. " +
                "Please add an internal setter or a test seam.");
        }
    }
}
