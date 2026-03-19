namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class FloatHintParameter : IHintParameter
    {
        public float Value { get; set; }
        public string Format { get; set; }

        public FloatHintParameter(float value, string format = "F2")
        {
            this.Value = value;
            this.Format = format;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.FloatHintParameter(this.Value, this.Format);
        }
    }
}
