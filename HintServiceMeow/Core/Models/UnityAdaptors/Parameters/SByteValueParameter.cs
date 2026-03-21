namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class SByteValueParameter : IParameter
    {
        public sbyte Value { get; set; }

        public SByteValueParameter(sbyte value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.SByteHintParameter(this.Value);
        }
    }
}
