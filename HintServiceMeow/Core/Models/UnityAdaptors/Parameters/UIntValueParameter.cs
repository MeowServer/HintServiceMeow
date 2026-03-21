namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class UIntValueParameter : IParameter
    {
        public uint Value { get; set; }

        public UIntValueParameter(uint value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.UIntHintParameter(this.Value);
        }
    }
}
