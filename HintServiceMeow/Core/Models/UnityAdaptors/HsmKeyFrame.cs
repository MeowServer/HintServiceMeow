namespace HintServiceMeow.Core.Models.UnityAdaptors
{
    public class HsmKeyFrame
    {
        public HsmKeyFrame(float time, float value, float inTangent = 0, float outTangent = 0)
        {
            this.Time = time;
            this.Value = value;
            this.InTangent = inTangent;
            this.OutTangent = outTangent;
        }

        public float Time { get; }

        public float Value { get; }

        public float InTangent { get; }

        public float OutTangent { get; }
    }
}
