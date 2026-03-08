using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.UniryAdaptors;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors
{
    internal class UnityAnimationCurveFactory : IAnimationCurveFactory
    {
        public IAnimationCurve Build(KeyFrame[] keyframes)
        {
            UnityEngine.Keyframe[] unityFrames = new UnityEngine.Keyframe[keyframes.Length];

            for (int i = 0; i < keyframes.Length; i++)
            {
                var kf = keyframes[i];
                unityFrames[i] = new UnityEngine.Keyframe(kf.Time, kf.Value, kf.InTangent, kf.OutTangent);
            }

            return new UnityAnimationCurve(new UnityEngine.AnimationCurve(unityFrames));
        }

        public IAnimationCurve BuildNormalized(EasingType type)
        {
            switch (type)
            {
                case EasingType.Linear:
                    return new UnityAnimationCurve(UnityEngine.AnimationCurve.Linear(0f, 0f, 1f, 1f));

                case EasingType.EaseIn:
                    return new UnityAnimationCurve(new UnityEngine.AnimationCurve(
                        new UnityEngine.Keyframe(0f, 0f) { outTangent = 0f },
                        new UnityEngine.Keyframe(1f, 1f) { inTangent = 2f }));

                case EasingType.EaseOut:
                    return new UnityAnimationCurve(new UnityEngine.AnimationCurve(
                        new UnityEngine.Keyframe(0f, 0f) { outTangent = 2f },
                        new UnityEngine.Keyframe(1f, 1f) { inTangent = 0f }));

                case EasingType.EaseInOut:
                default:
                    return new UnityAnimationCurve(UnityEngine.AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
            }
        }
    }
}
