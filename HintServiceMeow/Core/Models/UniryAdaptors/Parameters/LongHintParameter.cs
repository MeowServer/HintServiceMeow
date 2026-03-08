namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class LongHintParameter : IHintParameter
    {
        public long Value { get; set; }

        public LongHintParameter(long value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.LongHintParameter(this.Value);
        }
    }
}
