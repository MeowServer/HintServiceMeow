namespace HintServiceMeow.Core.Effects
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using UnityEngine;

    public class AlphaCurveHintEffect : IHintEffect
    {
        public AnimationCurve Curve { get; set; }
        public float StartScalar { get; set; }
        public float DurationScalar { get; set; }

        public AlphaCurveHintEffect(AnimationCurve curve, float startScalar = 0f, float durationScalar = 1f)
        {
            this.Curve = curve;
            this.StartScalar = startScalar;
            this.DurationScalar = durationScalar;
        }

        public HintEffect GetScpslHintEffect()
        {
            return new global::Hints.AlphaCurveHintEffect(this.Curve, this.StartScalar, this.DurationScalar);
        }
    }
}
