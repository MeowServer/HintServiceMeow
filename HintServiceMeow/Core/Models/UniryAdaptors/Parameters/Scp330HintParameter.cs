namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using InventorySystem.Items.Usables.Scp330;

    public class Scp330HintParameter : IHintParameter
    {
        public Scp330Translations.Entry Index { get; set; }

        public Scp330HintParameter(Scp330Translations.Entry index)
        {
            this.Index = index;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.Scp330HintParameter(this.Index);
        }
    }
}
