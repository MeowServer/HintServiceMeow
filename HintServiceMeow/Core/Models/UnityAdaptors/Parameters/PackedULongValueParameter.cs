namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class PackedULongValueParameter : IParameter
    {
        public ulong Value { get; set; }

        public PackedULongValueParameter(ulong value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.PackedULongHintParameter(this.Value);
        }
    }
}
