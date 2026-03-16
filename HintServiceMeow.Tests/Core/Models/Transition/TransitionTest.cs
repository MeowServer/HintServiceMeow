namespace HintServiceMeow.Tests.Core.Models.Transition
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Transition;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // ────────────────────────────────────────────────────────────────
    //  Transition Tests
    // ────────────────────────────────────────────────────────────────
    [TestClass]
    public class TransitionTests
    {
        private FakeCurveFactory factory = null!;

        [TestInitialize]
        public void Setup()
        {
            factory = new FakeCurveFactory();
            Transition.CurveFactory = factory;
        }

        // ---------- Duration ----------

        [TestMethod]
        public void Duration_SetPositiveValue_ReturnsSameValue()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 5f);
            t.Duration = 2.5f;
            Assert.AreEqual(2.5f, t.Duration);
        }

        [TestMethod]
        public void Duration_SetZero_ClampsToSmallPositive()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 1f);
            t.Duration = 0f;
            Assert.AreEqual(0.001f, t.Duration);
        }

        [TestMethod]
        public void Duration_SetNegative_ClampsToSmallPositive()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 1f);
            t.Duration = -10f;
            Assert.AreEqual(0.001f, t.Duration);
        }

        // ---------- Easing ----------

        [TestMethod]
        public void Easing_SetValue_ReturnsSetValue()
        {
            var t = Transition.Get(EasingType.EaseInOut);
            t.Easing = EasingType.EaseIn;
            Assert.AreEqual(EasingType.EaseIn, t.Easing);
        }

        [TestMethod]
        public void Easing_Set_InvokesBuildNormalized()
        {
            var t = Transition.Get(EasingType.EaseInOut);
            factory.BuildNormalizedCallCount = 0; // reset after Get()

            t.Easing = EasingType.EaseOut;

            Assert.AreEqual(1, factory.BuildNormalizedCallCount);
            Assert.AreEqual(EasingType.EaseOut, factory.LastBuildNormalizedType);
        }

        // ---------- NormalizedCurve ----------

        [TestMethod]
        public void NormalizedCurve_SetCustomCurve_SetsEasingToCustom()
        {
            var t = Transition.Get(EasingType.EaseInOut);
            var customCurve = new FakeCurve();

            t.NormalizedCurve = customCurve;

            Assert.AreEqual(EasingType.Custom, t.Easing);
            Assert.AreSame(customCurve, t.NormalizedCurve);
        }

        [TestMethod]
        public void NormalizedCurve_SetNull_SetsEasingToCustom()
        {
            var t = Transition.Get(EasingType.EaseInOut);
            t.NormalizedCurve = null;

            Assert.AreEqual(EasingType.Custom, t.Easing);
            Assert.IsNull(t.NormalizedCurve);
        }

        // ---------- Get (static factory) ----------

        [TestMethod]
        public void Get_WithEasingType_ReturnsCorrectDurationAndEasing()
        {
            var t = Transition.Get(EasingType.EaseIn, duration: 7f);

            Assert.AreEqual(7f, t.Duration);
            Assert.AreEqual(EasingType.EaseIn, t.Easing);
        }

        [TestMethod]
        public void Get_WithEasingType_DefaultParams()
        {
            var t = Transition.Get();

            Assert.AreEqual(3f, t.Duration);
            Assert.AreEqual(EasingType.EaseInOut, t.Easing);
        }

        [TestMethod]
        public void Get_WithCurve_ReturnsCustomEasing()
        {
            var customCurve = new FakeCurve();
            var t = Transition.Get(customCurve, duration: 4f);

            Assert.AreEqual(4f, t.Duration);
            Assert.AreEqual(EasingType.Custom, t.Easing);
            Assert.AreSame(customCurve, t.NormalizedCurve);
        }

        [TestMethod]
        public void Get_WithCurve_DefaultDuration()
        {
            var t = Transition.Get(new FakeCurve());
            Assert.AreEqual(3f, t.Duration);
        }

        // ---------- GetCurve ----------

        [TestMethod]
        public void GetCurve_ScalesKeyframeTimes()
        {
            // Normalized keys: t=0..1, duration=4 → expect t=0..4
            var t = Transition.Get(EasingType.EaseInOut, duration: 4f);

            // Call GetCurve – factory.Build will capture the scaled keys
            factory.BuildCallCount = 0;
            t.GetCurve(0f, 100f);

            Assert.AreEqual(1, factory.BuildCallCount);
            Assert.IsNotNull(factory.LastBuildKeys);

            var keys = factory.LastBuildKeys!;
            Assert.AreEqual(0f, keys[0].Time, 0.001f);      // 0 * 4
            Assert.AreEqual(4f, keys[1].Time, 0.001f);      // 1 * 4
        }

        [TestMethod]
        public void GetCurve_ScalesKeyframeValues()
        {
            // from=10, to=60 → range=50
            // normalized value 0 → 10, value 1 → 60
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);

            t.GetCurve(10f, 60f);
            var keys = factory.LastBuildKeys!;

            Assert.AreEqual(10f, keys[0].Value, 0.001f);    // 10 + 0*50
            Assert.AreEqual(60f, keys[1].Value, 0.001f);    // 10 + 1*50
        }

        [TestMethod]
        public void GetCurve_ScalesTangents()
        {
            // range=50, duration=2 → tangent multiplier = 50/2 = 25
            // original tangent=1 → scaled = 25
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);

            t.GetCurve(10f, 60f);
            var keys = factory.LastBuildKeys!;

            float expectedTangent = 1f * 50f / 2f; // 25
            Assert.AreEqual(expectedTangent, keys[0].InTangent, 0.001f);
            Assert.AreEqual(expectedTangent, keys[0].OutTangent, 0.001f);
        }

        [TestMethod]
        public void GetCurve_NullCurve_InitializesCurveAutomatically()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 1f);

            // Force internal curve to null via NormalizedCurve setter
            t.NormalizedCurve = null;
            factory.BuildNormalizedCallCount = 0;

            // GetCurve should detect null and rebuild
            t.GetCurve(0f, 1f);

            Assert.IsTrue(factory.BuildNormalizedCallCount >= 1,
                "Expected BuildNormalized to be called when internal curve is null.");
        }

        // ---------- Evaluate ----------

        [TestMethod]
        public void Evaluate_MidDuration_ReturnsInterpolatedValue()
        {
            // Linear FakeCurve: Evaluate(t) = t
            // elapsed=1, duration=2 → normalized=0.5 → start + 0.5*dif
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            float result = t.Evaluate(1f, 0f, 100f);

            Assert.AreEqual(50f, result, 0.01f);
        }

        [TestMethod]
        public void Evaluate_TimeBeyondDuration_ReturnsEnd()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            float result = t.Evaluate(5f, 0f, 100f);

            Assert.AreEqual(100f, result, 0.001f);
        }

        [TestMethod]
        public void Evaluate_NegativeTime_ReturnsStart()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            float result = t.Evaluate(-1f, 0f, 100f);

            Assert.AreEqual(0f, result, 0.001f);
        }

        [TestMethod]
        public void Evaluate_NullCurve_ReturnsEnd()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            t.NormalizedCurve = null;

            float result = t.Evaluate(1f, 10f, 90f);

            Assert.AreEqual(90f, result);
        }

        [TestMethod]
        public void Evaluate_AtTimeZero_ReturnsStart()
        {
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            float result = t.Evaluate(0f, 20f, 80f);

            // Linear curve: Evaluate(0/2) = 0 → 20 + 0*60 = 20
            Assert.AreEqual(20f, result, 0.001f);
        }

        [TestMethod]
        public void Evaluate_AtExactDuration_ReturnsEnd()
        {
            // time == duration → time > duration is false, so it goes through normal evaluation
            // normalized = duration/duration = 1 → curve.Evaluate(1) = 1 → start + dif
            var t = Transition.Get(EasingType.EaseInOut, duration: 2f);
            float result = t.Evaluate(2f, 0f, 50f);

            // time(2) > duration(2) is false, so: 0 + 50 * curve.Evaluate(1) = 50
            Assert.AreEqual(50f, result, 0.001f);
        }

        [TestMethod]
        public void Evaluate_ReversedRange_WorksCorrectly()
        {
            // start > end (e.g. fading out)
            var t = Transition.Get(EasingType.EaseInOut, duration: 4f);
            float result = t.Evaluate(2f, 100f, 0f);

            // normalized=0.5, dif=-100, 100 + (-100 * 0.5) = 50
            Assert.AreEqual(50f, result, 0.01f);
        }
    }
}