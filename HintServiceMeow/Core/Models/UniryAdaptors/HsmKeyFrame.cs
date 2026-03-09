namespace HintServiceMeow.Core.Models.UniryAdaptors
{
    public struct HsmKeyFrame
    {
        public HsmKeyFrame(float time, float value, float inTangent = 0, float outTangent = 0)
        {
            this.Time = time;
            this.Value = value;
            this.InTangent = inTangent;
            this.OutTangent = outTangent;
        }

        public float Time { get; set; }

        public float Value { get; set; }

        public float InTangent { get; set; }

        public float OutTangent { get; set; }
    }
}
