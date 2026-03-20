namespace HintServiceMeow.Core.Interface
{
    using global::Hints;

    /// <summary>
    /// Defines a contract for creating hint effects that can be used in SCP:SL.
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// Get <see cref="HintEffect"/> that can be used in SCP:SL.
        /// </summary>
        /// <returns>An instance of <see cref="HintEffect"/> that can be used in SCP:SL.</returns>
        HintEffect GetScpslHintEffect();
    }
}
