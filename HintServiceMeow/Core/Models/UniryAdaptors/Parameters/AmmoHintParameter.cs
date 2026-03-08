namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class AmmoHintParameter : IHintParameter
    {
        public byte Id { get; set; }

        public AmmoHintParameter(byte id)
        {
            this.Id = id;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.AmmoHintParameter(this.Id);
        }
    }
}
