namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ByteValueParameter : IParameter
    {
        public byte Value { get; set; }

        public ByteValueParameter(byte value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ByteHintParameter(this.Value);
        }
    }
}
