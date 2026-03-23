namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class PackedULongValueParameter : IParameter
    {
        public PackedULongValueParameter(ulong value)
        {
            this.Value = value;
        }

        public ulong Value { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.PackedULongHintParameter(this.Value);
        }
    }
}
