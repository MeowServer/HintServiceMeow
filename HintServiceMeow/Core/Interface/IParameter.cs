namespace HintServiceMeow.Core.Interface
{
    using global::Hints;

    public interface IParameter
    {
        /// <summary>
        /// Get an instance of <see cref="HintParameter"/> that can be used in SCP:SL.
        /// </summary>
        /// <returns>A <see cref="HintParameter"/> instance that can be used in SCP:SL.</returns>
        HintParameter GetScpslHintParameter();
    }
}
