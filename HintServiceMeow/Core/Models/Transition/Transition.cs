namespace HintServiceMeow.Core.Models.Transition
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.UnityAdaptors;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;

    public class Transition
    {
        private readonly object @lock = new object();
        private float duration;
        private IAnimationCurve curve;
        private EasingType easing;

        private Transition()
        {
        }

        /// <summary> Gets or sets the duration of the transition in seconds. </summary>
        public float Duration
        {
            get
            {
                lock (@lock)
                {
                    return duration;
                }
            }

            set
            {
                lock (@lock)
                {
                    duration = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the easing type used. The curve use by transition will be generated based on this easing type.
        /// If set to <see cref="EasingType.Custom"/>, the curve will be defaultly set to an EaseInOut curve.
        /// </summary>
        /// <remarks>The easing function determines the rate of change of a value over time, allowing for
        /// smooth transitions.</remarks>
        public EasingType Easing
        {
            get
            {
                lock (@lock)
                {
                    return easing;
                }
            }

            set
            {
                lock (@lock)
                {
                    curve = CurveFactory.BuildNormalized(value);
                    easing = value;
                }
            }
        }

        public IAnimationCurve? NormalizedCurve
        {
            get
            {
                lock (@lock)
                {
                    return curve;
                }
            }

            set
            {
                lock (@lock)
                {
                    curve = value;
                    easing = EasingType.Custom;
                }
            }
        }

        internal static IAnimationCurveFactory CurveFactory { get; set; } = new UnityAnimationCurveFactory();

        public static Transition Get(IAnimationCurve normalizedCurve, float duration = 3f)
        {
            return new Transition()
            {
                curve = normalizedCurve,
                easing = EasingType.Custom,
                duration = duration,
            };
        }

        public static Transition Get(EasingType type = EasingType.EaseInOut, float duration = 3f)
        {
            return new Transition()
            {
                curve = CurveFactory.BuildNormalized(type),
                easing = type,
                duration = duration,
            };
        }

        internal IAnimationCurve GetCurve(float from, float to)
        {
            lock (@lock)
            {
                // If has a custom normalized curve, scale and return it.
                if (curve != null)
                {
                    float range = to - from;
                    HsmKeyFrame[] keys = curve.Keys;
                    HsmKeyFrame[] scaled = new HsmKeyFrame[keys.Length];

                    for (int i = 0; i < keys.Length; i++)
                    {
                        scaled[i] = new HsmKeyFrame(
                            time: keys[i].Time * duration,
                            value: from + (keys[i].Value * range),
                            inTangent: keys[i].InTangent * range / duration,
                            outTangent: keys[i].OutTangent * range / duration);
                    }

                    return CurveFactory.Build(scaled);
                }

                // If no custom curve, generate curve based on easing type.
                return CurveFactory.BuildNormalized(easing);
            }
        }

        internal float Evaluate(float time, float start, float end)
        {
            lock (@lock)
            {
                if (curve is null)
                    return end;

                if (time > duration)
                    return end;
                if (time < 0)
                    return start;

                float dif = end - start;

                return start + (dif * curve.Evaluate(time / duration));
            }
        }
    }
}