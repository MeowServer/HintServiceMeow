namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class LongValueParameter : IParameter
    {
        public long Value { get; set; }

        public LongValueParameter(long value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.LongHintParameter(this.Value);
        }
    }
}
