namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class DoubleHintParameter : IHintParameter
    {
        public double Value { get; set; }
        public string Format { get; set; }

        public DoubleHintParameter(double value, string format)
        {
            this.Value = value;
            this.Format = format;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.DoubleHintParameter(this.Value, this.Format);
        }
    }
}
