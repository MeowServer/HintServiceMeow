namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ShortHintParameter : IHintParameter
    {
        public short Value { get; set; }

        public ShortHintParameter(short value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ShortHintParameter(this.Value);
        }
    }
}
