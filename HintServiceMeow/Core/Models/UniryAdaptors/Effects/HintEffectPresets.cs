namespace HintServiceMeow.Core.Effects
{
    using UnityEngine;

    internal static class HintEffectPresets
    {
        public static Keyframe[] CreateBumpKeyframes(float floorValue, float bumpValue, int count, float duration = 1f)
        {
            return global::Hints.HintEffectPresets.CreateBumpKeyframes(floorValue, bumpValue, count, duration);
        }

        public static AnimationCurve CreateBumpCurve(float floorValue, float bumpValue, int count, float duration = 1f)
        {
            return global::Hints.HintEffectPresets.CreateBumpCurve(floorValue, bumpValue, count, duration);
        }

        public static AnimationCurve CreateTrailingBumpCurve(float floorValue, float bumpValue, int count, float startTrailPercent, float duration = 1f)
        {
            return global::Hints.HintEffectPresets.CreateTrailingBumpCurve(floorValue, bumpValue, count, startTrailPercent, duration);
        }

        public static AlphaCurveHintEffect FadeIn(float durationScalar = 1f, float startScalar = 0f, float iterations = 1f)
        {
            AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, iterations, 1f);
            curve.postWrapMode = WrapMode.Loop;
            return new AlphaCurveHintEffect(curve, startScalar, durationScalar);
        }

        public static AlphaCurveHintEffect FadeOut(float durationScalar = 1f, float startScalar = 0f, float iterations = 1f)
        {
            AnimationCurve curve = AnimationCurve.EaseInOut(0f, 1f, iterations, 0f);
            curve.postWrapMode = WrapMode.Loop;
            return new AlphaCurveHintEffect(curve, startScalar, durationScalar);
        }

        public static Interface.IHintEffect[] FadeInAndOut(float window, float durationScalar = 1f, float startScalar = 0f)
        {
            float num = (durationScalar - window) / 2f;
            return new Interface.IHintEffect[]
            {
                FadeIn(num, startScalar, 1f),
                FadeOut(num, startScalar + durationScalar - num, 1f)
            };
        }

        public static AlphaCurveHintEffect PulseAlpha(float floorValue, float peakValue, float iterationScalar = 1f, float startOffset = 0f)
        {
            return new AlphaCurveHintEffect(CreateBumpCurve(floorValue, peakValue, 1, iterationScalar), startOffset, 1f);
        }

        public static AlphaCurveHintEffect TrailingPulseAlpha(float floorValue, float peakValue, float startTrailScalar, float iterationScalar = 1f, float startScalar = 0f, int count = 1)
        {
            return new AlphaCurveHintEffect(CreateTrailingBumpCurve(floorValue, peakValue, count, startTrailScalar, iterationScalar), startScalar, 1f);
        }
    }
}
