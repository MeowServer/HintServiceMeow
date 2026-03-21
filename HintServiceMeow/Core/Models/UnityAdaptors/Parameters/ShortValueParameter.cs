namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ShortValueParameter : IParameter
    {
        public short Value { get; set; }

        public ShortValueParameter(short value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ShortHintParameter(this.Value);
        }
    }
}
