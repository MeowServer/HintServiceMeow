namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using InventorySystem.Items.Usables.Scp330;

    public class Scp330Parameter : IParameter
    {
        public Scp330Parameter(Scp330Translations.Entry index)
        {
            this.Index = index;
        }

        public Scp330Translations.Entry Index { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.Scp330HintParameter(this.Index);
        }
    }
}
