namespace HintServiceMeow.Core.Models.Arguments
{
    /// <summary>
    /// Represents the arguments required by a hint parser.
    /// </summary>
    public class HintParserArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HintParserArgument"> with the specified hint collection and screen XY ratio.
        /// </summary>
        /// <param name="collection">The collection of hitns to be parsed.</param>
        /// <param name="screenXyRatio">The screen xy ratio used by parser.</param>
        public HintParserArgument(HintCollection collection, float screenXyRatio)
        {
            Collection = collection;
            ScreenXyRatio = screenXyRatio;
        }

        /// <summary>
        /// Gets the collection of hints to be parsed.
        /// </summary>
        public HintCollection Collection { get; }

        /// <summary>
        /// Gets the xy ratio of the screen.
        /// </summary>
        public float ScreenXyRatio { get; }
    }
}
