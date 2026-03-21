namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ULongValueParameter : IParameter
    {
        public ulong Value { get; set; }

        public ULongValueParameter(ulong value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ULongHintParameter(this.Value);
        }
    }
}
