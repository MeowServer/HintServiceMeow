namespace HintServiceMeow.Core.Models.UnityAdaptors.Effects
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    /// <summary>
    /// Represents an effect that applies a transparency level.
    /// </summary>
    public class TransparencyEffect : IEffect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransparencyEffect"/> class.
        /// parameters.
        /// </summary>
        /// <param name="alpha">The transparency level to apply. Must be between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
        /// <param name="startScalar">The starting point of the effect, as a scalar value.</param>
        /// <param name="durationScalar">The duration of the effect, as a scalar value.</param>
        public TransparencyEffect(float alpha, float startScalar = 0f, float durationScalar = 1f)
        {
            this.Transparency = alpha;
            this.StartPoint = startScalar;
            this.Duration = durationScalar;
        }

        /// <summary>
        /// Gets or sets the transparency level.
        /// </summary>
        public float Transparency { get; set; }

        /// <summary>
        /// Gets or sets the initial start point.
        /// </summary>
        public float StartPoint { get; set; }

        /// <summary>
        /// Gets or sets the duration, in seconds.
        /// </summary>
        public float Duration { get; set; }

        /// <inheritdoc/>
        public HintEffect GetScpslHintEffect()
        {
            return new global::Hints.AlphaEffect(this.Transparency, this.StartPoint, this.Duration);
        }
    }
}
