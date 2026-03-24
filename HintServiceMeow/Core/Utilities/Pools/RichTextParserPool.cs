namespace HintServiceMeow.Core.Utilities.Pools
{
    using HintServiceMeow.Core.Utilities.Parser;

    /// <summary>
    /// A pool for reusing <see cref="RichTextParser"/> instances to reduce allocations.
    /// </summary>
    internal class RichTextParserPool : PoolBase<RichTextParser>
    {
        /// <summary>
        /// Gets the shared singleton instance of the <see cref="RichTextParserPool"/>.
        /// </summary>
        public static RichTextParserPool Instance { get; } = new();

        /// <inheritdoc/>
        protected override void Reset(RichTextParser parser)
        {
        }

        /// <inheritdoc/>
        protected override RichTextParser Create() => new();
    }
}
