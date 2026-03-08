namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class IntHintParameter : IHintParameter
    {
        public int Value { get; set; }

        public IntHintParameter(int value)
        {
            this.Value = value;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.IntHintParameter(this.Value);
        }
    }
}
