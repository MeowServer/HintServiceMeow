namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class StringHintParameter : IHintParameter
    {
        public string Value { get; set; }

        public StringHintParameter(string value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.StringHintParameter(this.Value);
        }
    }
}
