namespace HintServiceMeow.Core.Models.Arguments
{
    public class HintParserArgument
    {
        public HintCollection Collection { get; }

        public float ScreenXyRatio { get; }

        public HintParserArgument(HintCollection collection, float screenXyRatio)
        {
            Collection = collection;
            ScreenXyRatio = screenXyRatio;
        }
    }
}
