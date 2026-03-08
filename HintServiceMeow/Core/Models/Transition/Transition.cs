namespace HintServiceMeow.Core.Models.Transition
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.UniryAdaptors;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;

    public class Transition
    {
        private readonly object @lock = new object();
        private float duration;
        private IAnimationCurve? customCurve;
        private EasingType easing;

        private Transition()
        {
        }

        internal static IAnimationCurveFactory CurveFactory { get; set; } = new UnityAnimationCurveFactory();

        /// <summary> Gets or sets the duration of the transition in seconds. </summary>
        public float Duration
        {
            get { lock (@lock) { return duration; } }
            set { lock (@lock) { duration = value; } }
        }

        /// <summary> Gets or sets the easing type. Custom if a custom curve was provided. </summary>
        public EasingType Easing
        {
            get
            {
                lock (@lock) { return easing; }
            }

            set
            {
                lock (@lock)
                {
                    customCurve = CurveFactory.BuildNormalized(value);
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
                    if (customCurve != null)
                        return customCurve;

                    return CurveFactory.BuildNormalized(easing);
                }
            }

            set
            {
                lock (@lock)
                {
                    customCurve = value;
                    easing = EasingType.Custom;
                }
            }
        }

        public IAnimationCurve? CustomCurve
        {
            get { lock (@lock) { return customCurve; } }
        }

        public static Transition Get(IAnimationCurve normalizedCurve, float duration = 3f)
        {
            return new Transition()
            {
                customCurve = normalizedCurve,
                Easing = EasingType.Custom,
                Duration = duration,
            };
        }

        public static Transition Get(EasingType type = EasingType.EaseInOut, float duration = 3f)
        {
            return new Transition()
            {
                customCurve = CurveFactory.BuildNormalized(type),
                Easing = type,
                Duration = duration,
            };
        }

        internal IAnimationCurve GetCurve(float from, float to)
        {
            lock (@lock)
            {
                // If has a custom normalized curve, scale and return it.
                if (customCurve != null)
                {
                    float range = to - from;
                    KeyFrame[] keys = customCurve.KeyFrames;
                    KeyFrame[] scaled = new KeyFrame[keys.Length];

                    for (int i = 0; i < keys.Length; i++)
                    {
                        scaled[i] = new KeyFrame(
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

        internal float Evluate(float time, float start, float end)
        {
            lock (@lock)
            {
                if (customCurve is null)
                    return end;

                if (time > duration)
                    return end;
                if (time < 0)
                    return start;

                float dif = end - start;

                return start + (dif * customCurve.Evaluate(time / duration));
            }
        }
    }
}