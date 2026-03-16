namespace HintServiceMeow.Tests.Core.Models.Transition
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.UnityAdaptors;

    /// <summary>Fake factory that records every call and returns <see cref="FakeCurve"/> instances.</summary>
    internal sealed class FakeCurveFactory : IAnimationCurveFactory
    {
        public int BuildNormalizedCallCount { get; set; }
        public EasingType? LastBuildNormalizedType { get; set; }

        public int BuildCallCount { get; set; }
        public HsmKeyFrame[]? LastBuildKeys { get; private set; }

        /// <summary>Optional override; when set, <see cref="BuildNormalized"/> returns it.</summary>
        public IAnimationCurve? NextNormalizedCurve { get; set; }

        /// <summary>Optional override; when set, <see cref="Build"/> returns it.</summary>
        public IAnimationCurve? NextBuildCurve { get; set; }

        public IAnimationCurve BuildNormalized(EasingType type)
        {
            BuildNormalizedCallCount++;
            LastBuildNormalizedType = type;
            return NextNormalizedCurve ?? new FakeCurve();
        }

        public IAnimationCurve Build(HsmKeyFrame[] keys)
        {
            BuildCallCount++;
            LastBuildKeys = keys;
            return NextBuildCurve ?? new FakeCurve(keys);
        }
    }
}
