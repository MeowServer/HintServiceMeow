namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class PackedLongHintParameter : IHintParameter
    {
        public long Value { get; set; }

        public PackedLongHintParameter(long value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.PackedLongHintParameter(this.Value);
        }
    }
}
