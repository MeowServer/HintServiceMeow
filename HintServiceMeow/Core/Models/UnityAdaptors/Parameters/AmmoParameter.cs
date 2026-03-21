namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class AmmoParameter : IParameter
    {
        public byte Id { get; set; }

        public AmmoParameter(byte id)
        {
            this.Id = id;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.AmmoHintParameter(this.Id);
        }
    }
}
