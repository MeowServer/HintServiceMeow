namespace HintServiceMeow.Core.Utilities.UnityAdaptors
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.UnityAdaptors;

    /// <summary>
    /// Represents the default factory for creating animation curves.
    /// </summary>
    internal class UnityAnimationCurveFactory : IAnimationCurveFactory
    {
        /// <summary>
        /// Creates an animation curve from the specified array of keyframes.
        /// </summary>
        /// <param name="keyframes">An array of keyframes that define the points and tangents of the animation curve.</param>
        /// <returns>An object representing the constructed animation curve based on the provided keyframes.</returns>
        public IAnimationCurve Build(HsmKeyFrame[] keyframes)
        {
            UnityEngine.Keyframe[] unityFrames = new UnityEngine.Keyframe[keyframes.Length];

            for (int i = 0; i < keyframes.Length; i++)
            {
                var kf = keyframes[i];
                unityFrames[i] = new UnityEngine.Keyframe(kf.Time, kf.Value, kf.InTangent, kf.OutTangent);
            }

            return new UnityAnimationCurve(new UnityEngine.AnimationCurve(unityFrames));
        }

        /// <summary>
        /// Builds a normalized animation curve corresponding to the specified easing type.
        /// </summary>
        /// <param name="type">The type of easing function to use when constructing the animation curve.</param>
        /// <returns>An animation curve normalized to the range [0, 1] for both time and value, representing the specified easing
        /// behavior.</returns>
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
