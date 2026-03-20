namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    public class ItemCategoryParameter : IParameter
    {
        public ItemCategory Category { get; set; }

        public ItemCategoryParameter(ItemCategory category)
        {
            this.Category = category;
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.ItemCategoryHintParameter(this.Category);
        }
    }
}
