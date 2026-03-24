namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class UIntValueParameter : IParameter
    {
        public UIntValueParameter(uint value)
        {
            this.Value = value;
        }

        public uint Value { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.UIntHintParameter(this.Value);
        }
    }
}
