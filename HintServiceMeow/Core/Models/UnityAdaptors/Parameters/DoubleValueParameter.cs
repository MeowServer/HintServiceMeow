namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class DoubleValueParameter : IParameter
    {
        public DoubleValueParameter(double value, string format = "F2")
        {
            this.Value = value;
            this.Format = format;
        }

        public double Value { get; set; }

        public string Format { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.DoubleHintParameter(this.Value, this.Format);
        }
    }
}
