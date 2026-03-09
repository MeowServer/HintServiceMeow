namespace HintServiceMeow.Core.Effects
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class AlphaEffect : IHintEffect
    {
        public float Alpha { get; set; }
        public float StartScalar { get; set; }
        public float DurationScalar { get; set; }

        public AlphaEffect(float alpha, float startScalar = 0f, float durationScalar = 1f)
        {
            this.Alpha = alpha;
            this.StartScalar = startScalar;
            this.DurationScalar = durationScalar;
        }

        public HintEffect GetScpslHintEffect()
        {
            return new global::Hints.AlphaEffect(this.Alpha, this.StartScalar, this.DurationScalar);
        }
    }
}
