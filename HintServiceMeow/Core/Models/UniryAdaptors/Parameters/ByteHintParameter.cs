namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ByteHintParameter : IHintParameter
    {
        public byte Value { get; set; }

        public ByteHintParameter(byte value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ByteHintParameter(this.Value);
        }
    }
}
