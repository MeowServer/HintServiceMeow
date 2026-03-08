namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using UnityEngine;

    public class AnimationCurveHintParameter : IHintParameter
    {
        public double Offset { get; set; }

        public AnimationCurve Curve { get; set; }

        public string Format { get; set; }

        public bool Integral { get; set; }

        public AnimationCurveHintParameter(double offset, AnimationCurve curve, string format, bool integral)
        {
            this.Offset = offset;
            this.Curve = curve;
            this.Format = format;
            this.Integral = integral;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.AnimationCurveHintParameter(this.Offset, this.Curve, this.Format, this.Integral);
        }
    }
}
