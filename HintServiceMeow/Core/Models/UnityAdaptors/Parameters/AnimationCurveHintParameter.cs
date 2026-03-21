namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Extension;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;
    using UnityEngine;

    public class AnimationCurveHintParameter : IHintParameter
    {
        public double Offset { get; set; }

        public IAnimationCurve Curve { get; set; }

        public string Format { get; set; }

        public bool Integral { get; set; }

        public AnimationCurveHintParameter(double offset, IAnimationCurve curve, string format, bool integral)
        {
            this.Offset = offset;
            this.Curve = curve;
            this.Format = format;
            this.Integral = integral;
        }

        public AnimationCurveHintParameter(IAnimationCurve curve, string format = "F1") : this(NetworkTimeCache.Time, curve, format, false)
        {
        }

        public HintParameter GetScpslHintParameter()
        {
            Keyframe[] keyframes = new Keyframe[Curve.Keys.Length];
            for (int i = 0; i < Curve.Keys.Length; i++)
            {
                var kf = Curve.Keys[i];
                keyframes[i] = new Keyframe(kf.Time, kf.Value, kf.InTangent, kf.OutTangent);
            }

            AnimationCurve curve = new AnimationCurve(keyframes);
            curve.preWrapMode = Curve.PreWrapMode.ToUnityWrapMode();
            curve.postWrapMode = Curve.PostWrapMode.ToUnityWrapMode();

            return new global::Hints.AnimationCurveHintParameter(this.Offset, curve, this.Format, this.Integral);
        }
    }
}
