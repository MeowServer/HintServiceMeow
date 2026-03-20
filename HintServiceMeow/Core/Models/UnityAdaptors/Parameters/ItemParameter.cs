namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ItemParameter : IParameter
    {
        public ItemType Item { get; set; }

        public ItemParameter(ItemType item)
        {
            this.Item = item;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ItemHintParameter(this.Item);
        }
    }
}
