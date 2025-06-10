namespace HintServiceMeow.Core.Enum
{
    public enum DelayType
    {
        /// <summary>
        /// Only keep the fastest scheduled action time
        /// </summary>
        KeepFastest,
        /// <summary>
        /// Only keep the latest scheduled action time
        /// </summary>
        KeepSlowest,
        /// <summary>
        /// Update the action time without comparing
        /// </summary>
        Override
    }
}
