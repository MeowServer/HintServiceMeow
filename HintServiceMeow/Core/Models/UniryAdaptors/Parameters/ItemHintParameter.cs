namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using InventorySystem.Items;

    public class ItemHintParameter : IHintParameter
    {
        public ItemType Item { get; set; }

        public ItemHintParameter(ItemType item)
        {
            this.Item = item;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ItemHintParameter(this.Item);
        }
    }
}
