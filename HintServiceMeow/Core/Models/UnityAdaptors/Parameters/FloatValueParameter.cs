namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class FloatValueParameter : IParameter
    {
        public FloatValueParameter(float value, string format = "F2")
        {
            this.Value = value;
            this.Format = format;
        }

        public float Value { get; set; }

        public string Format { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.FloatHintParameter(this.Value, this.Format);
        }
    }
}
