namespace HintServiceMeow.Core.Utilities.Pools
{
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// A pool for reusing <see cref="Hint"/> instances to reduce allocations.
    /// </summary>
    internal class HintPool : PoolBase<Hint>
    {
        /// <summary>
        /// Gets the shared singleton instance of the <see cref="HintPool"/>.
        /// </summary>
        public static HintPool Instance { get; } = new();

        /// <inheritdoc/>
        protected override void Reset(Hint hint) => hint.ResetFields();

        /// <inheritdoc/>
        protected override Hint Create() => new Hint();
    }
}
