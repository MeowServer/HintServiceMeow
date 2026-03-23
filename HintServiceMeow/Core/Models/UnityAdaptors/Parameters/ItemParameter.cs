namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ItemParameter : IParameter
    {
        public ItemParameter(ItemType item)
        {
            this.Item = item;
        }

        public ItemType Item { get; set; }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ItemHintParameter(this.Item);
        }
    }
}
