namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class TextParameter : IParameter
    {
        public string Value { get; set; }

        public TextParameter(string value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.StringHintParameter(this.Value);
        }
    }
}
