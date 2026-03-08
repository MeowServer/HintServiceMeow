namespace HintServiceMeow.Core.Models.Transition
{
    using HintServiceMeow.Core.Enum;
    using UnityEngine;

    public class Transition
    {
        private readonly object @lock = new object();
        private float duration;
        private AnimationCurve? customCurve;
        private EasingType easing;

        private Transition()
        {
        }

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
                    customCurve = GetNormalizedCurve(value);
                    easing = value;
                }
            }
        }

        public AnimationCurve? NormalizedCurve
        {
            get
            {
                lock (@lock)
                {
                    if (customCurve != null)
                        return customCurve;

                    return GetNormalizedCurve(easing);
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

        public AnimationCurve? CustomCurve
        {
            get { lock (@lock) { return customCurve; } }
        }

        public static Transition Get(AnimationCurve normalizedCurve)
        {
            return new Transition()
            {
                customCurve = normalizedCurve,
                Easing = EasingType.Custom,
            };
        }

        public static Transition Get(EasingType type = EasingType.EaseInOut)
        {
            return new Transition()
            {
                customCurve = GetNormalizedCurve(type),
                Easing = type,
            };
        }

        internal AnimationCurve GetCurve(float from, float to)
        {
            lock (@lock)
            {
                // If has a custom normalized curve, scale and return it.
                if (customCurve != null)
                {
                    float range = to - from;
                    Keyframe[] keys = customCurve.keys;
                    Keyframe[] scaled = new Keyframe[keys.Length];

                    for (int i = 0; i < keys.Length; i++)
                    {
                        scaled[i] = new Keyframe(
                            time: keys[i].time * duration,
                            value: from + (keys[i].value * range),
                            inTangent: keys[i].inTangent * range / duration,
                            outTangent: keys[i].outTangent * range / duration);
                    }

                    return new AnimationCurve(scaled);
                }

                // If no custom curve, generate curve based on easing type.
                switch (easing)
                {
                    case EasingType.Linear:
                        return AnimationCurve.Linear(0f, from, duration, to);

                    case EasingType.EaseIn:
                        return new AnimationCurve(
                            new Keyframe(0f, from) { outTangent = 0f },
                            new Keyframe(duration, to) { inTangent = (to - from) * 2f / duration });

                    case EasingType.EaseOut:
                        return new AnimationCurve(
                            new Keyframe(0f, from) { outTangent = (to - from) * 2f / duration },
                            new Keyframe(duration, to) { inTangent = 0f });

                    case EasingType.EaseInOut:
                    default:
                        return AnimationCurve.EaseInOut(0f, from, duration, to);
                }
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

        private static AnimationCurve GetNormalizedCurve(EasingType type)
        {
            switch (type)
            {
                case EasingType.Linear:
                    return AnimationCurve.Linear(0f, 0f, 1f, 1f);

                case EasingType.EaseIn:
                    return new AnimationCurve(
                        new Keyframe(0f, 0f) { outTangent = 0f },
                        new Keyframe(1f, 1f) { inTangent = 2f });

                case EasingType.EaseOut:
                    return new AnimationCurve(
                        new Keyframe(0f, 0f) { outTangent = 2f },
                        new Keyframe(1f, 1f) { inTangent = 0f });

                case EasingType.EaseInOut:
                default:
                    return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
    }
}