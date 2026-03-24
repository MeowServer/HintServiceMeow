namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class UShortValueParameter : IParameter
    {
        public UShortValueParameter(ushort value)
        {
            this.Value = value;
        }

        public ushort Value { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.UShortHintParameter(this.Value);
        }
    }
}
