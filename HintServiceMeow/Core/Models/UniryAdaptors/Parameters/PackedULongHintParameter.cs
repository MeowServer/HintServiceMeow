namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class PackedULongHintParameter : IHintParameter
    {
        public ulong Value { get; set; }

        public PackedULongHintParameter(ulong value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.PackedULongHintParameter(this.Value);
        }
    }
}
