namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class SByteValueParameter : IParameter
    {
        public SByteValueParameter(sbyte value)
        {
            this.Value = value;
        }

        public sbyte Value { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.SByteHintParameter(this.Value);
        }
    }
}
