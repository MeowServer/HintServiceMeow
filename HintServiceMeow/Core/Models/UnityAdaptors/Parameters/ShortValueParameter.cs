namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ShortValueParameter : IParameter
    {
        public ShortValueParameter(short value)
        {
            this.Value = value;
        }

        public short Value { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ShortHintParameter(this.Value);
        }
    }
}
