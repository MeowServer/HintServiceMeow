namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class SSKeybindHintParameter : IHintParameter
    {
        public int Value { get; set; }
        public string Format { get; set; }

        public const string SettingNotFound = "SERVER SETTING NOT FOUND";
        public const string KeyNotAssigned = "KEY NOT ASSIGNED";
        public const string DefaultKeybindFormat = "[{0}]";

        public SSKeybindHintParameter(int value, string format)
        {
            this.Value = value;
            this.Format = format;
        }

        public SSKeybindHintParameter(int value) : this(value, DefaultKeybindFormat)
        {
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.SSKeybindHintParameter(this.Value, this.Format);
        }
    }
}
