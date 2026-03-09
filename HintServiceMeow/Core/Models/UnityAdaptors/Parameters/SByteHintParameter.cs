namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class SByteHintParameter : IHintParameter
    {
        public sbyte Value { get; set; }

        public SByteHintParameter(sbyte value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.SByteHintParameter(this.Value);
        }
    }
}
