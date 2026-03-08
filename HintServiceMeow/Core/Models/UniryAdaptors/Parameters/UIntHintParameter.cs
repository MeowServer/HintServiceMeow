namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class UIntHintParameter : IHintParameter
    {
        public uint Value { get; set; }

        public UIntHintParameter(uint value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.UIntHintParameter(this.Value);
        }
    }
}
