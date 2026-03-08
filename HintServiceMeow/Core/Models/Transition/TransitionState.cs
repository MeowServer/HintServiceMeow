namespace HintServiceMeow.Core.Models.Transition
{
    using HintServiceMeow.Core.Utilities.UnityAdaptors;

    /// <summary>
    /// Snapshot of an active property transition.
    /// Created automatically when a property with a <see cref="Models.Transition.Transition"/> config changes.
    /// HintParser reads this to generate <c>AnimationCurveHintParameter</c>.
    /// </summary>
    internal class TransitionState
    {
        private readonly object @lock = new object();

        /// <summary>Initializes a new transition state.</summary>
        /// <param name="transition">The transition configuration.</param>
        /// <param name="fromValue">Value before the change.</param>
        /// <param name="toValue">Value after the change.</param>
        public TransitionState(Transition transition, float fromValue, float toValue)
        {
            Transition = transition;
            StartTime = NetworkTimeCache.Time;
            FromValue = fromValue;
            ToValue = toValue;
        }

        /// <summary> Gets the transition configuration that created this state. </summary>
        public Transition Transition { get; }

        /// <summary> Gets NetworkTimeCache.Time when the transition started. </summary>
        public double StartTime { get; }

        /// <summary> Gets the value before the change. </summary>
        public float FromValue { get; }

        /// <summary> Gets the target value. </summary>
        public float ToValue { get; }

        /// <summary> Gets shortcut for transition duration. </summary>
        public float Duration => Transition.Duration;

        /// <summary> Gets a value indicating whether this transition has finished. </summary>
        public bool IsExpired
        {
            get
            {
                lock (@lock)
                {
                    return (NetworkTimeCache.Time - StartTime) >= Duration;
                }
            }
        }

        public float CurrentValue
        {
            get
            {
                lock (@lock)
                {
                    if (IsExpired)
                        return ToValue;

                    float elapsed = (float)(NetworkTimeCache.Time - StartTime);
                    return Transition.Evluate(elapsed, FromValue, ToValue);
                }
            }
        }
    }
}