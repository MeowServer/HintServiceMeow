namespace HintServiceMeow.Core.Enum
{
    /// <summary>
    /// Easing types, analogous to CSS transition-timing-function.
    /// </summary>
    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,

        /// <summary> Indicates a custom AnimationCurve is being used. </summary>
        Custom,
    }
}
