namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class PackedLongValueParameter : IParameter
    {
        public long Value { get; set; }

        public PackedLongValueParameter(long value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.PackedLongHintParameter(this.Value);
        }
    }
}
