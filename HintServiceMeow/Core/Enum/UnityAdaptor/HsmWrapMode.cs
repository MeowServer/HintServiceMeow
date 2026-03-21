namespace HintServiceMeow.Core.Enum.UnityAdaptor
{
    /// <summary>
    /// Compatibility enum for <see cref="UnityEngine.WrapMode"/>. Used in <see cref="Core.Models.UnityAdaptors.UnityAnimationCurve"/> to avoid direct dependency on Unity types."/>
    /// Once represent <see cref="UnityEngine.WrapMode.Once"/>, <see cref="UnityEngine.WrapMode.Clamp"/>, and <see cref="UnityEngine.WrapMode.ClampForever"/>. Loop represent <see cref="UnityEngine.WrapMode.Loop"/>, PingPong represent <see cref="UnityEngine.WrapMode.PingPong"/>, Default represent <see cref="UnityEngine.WrapMode.Default"/>."/>
    /// </summary>
    public enum HsmWrapMode
    {
        Once = 1,
        Loop = 2,
        PingPong = 4,
        Default = 0,
    }
}
