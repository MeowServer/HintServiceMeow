namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class IntValueParameter : IParameter
    {
        public int Value { get; set; }

        public IntValueParameter(int value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.IntHintParameter(this.Value);
        }
    }
}
