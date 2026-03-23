namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ByteValueParameter : IParameter
    {
        public ByteValueParameter(byte value)
        {
            this.Value = value;
        }

        public byte Value { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ByteHintParameter(this.Value);
        }
    }
}
