namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class TextParameter : IParameter
    {
        public TextParameter(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.StringHintParameter(this.Value);
        }
    }
}
