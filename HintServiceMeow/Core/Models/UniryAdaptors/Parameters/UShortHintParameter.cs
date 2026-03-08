namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class UShortHintParameter : IHintParameter
    {
        public ushort Value { get; set; }

        public UShortHintParameter(ushort value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.UShortHintParameter(this.Value);
        }
    }
}
