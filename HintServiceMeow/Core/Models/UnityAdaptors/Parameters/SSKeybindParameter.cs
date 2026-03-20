namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class SSKeybindParameter : IParameter
    {
        public int Value { get; set; }
        public string Format { get; set; }

        public const string SettingNotFound = "SERVER SETTING NOT FOUND";
        public const string KeyNotAssigned = "KEY NOT ASSIGNED";
        public const string DefaultKeybindFormat = "[{0}]";

        public SSKeybindParameter(int value, string format)
        {
            this.Value = value;
            this.Format = format;
        }

        public SSKeybindParameter(int value) : this(value, DefaultKeybindFormat)
        {
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.SSKeybindHintParameter(this.Value, this.Format);
        }
    }
}
