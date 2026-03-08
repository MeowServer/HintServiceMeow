namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ULongHintParameter : IHintParameter
    {
        public ulong Value { get; set; }

        public ULongHintParameter(ulong value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ULongHintParameter(this.Value);
        }
    }
}
