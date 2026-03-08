namespace HintServiceMeow.Core.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ItemCategoryHintParameter : IHintParameter
    {
        public ItemCategory Category { get; set; }

        public ItemCategoryHintParameter(ItemCategory category)
        {
            this.Category = category;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ItemCategoryHintParameter(this.Category);
        }
    }
}
