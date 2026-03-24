namespace HintServiceMeow.Core.Extension
{
    using HintServiceMeow.Core.Enum.UnityAdaptor;
    using UnityEngine;

    /// <summary>
    /// Extension method for <see cref="WrapMode"/> and <see cref="HsmWrapMode"/> to convert between the two.
    /// </summary>
    public static class WrapModeExtension
    {
        /// <summary>
        /// Converts a value of the <see cref="WrapMode"/> to the corresponding <see cref="HsmWrapMode"/> value.
        /// </summary>
        /// <remarks>If the value of rm is WrapMode.ClampForever, the method returns
        /// HsmWrapMode.Once.</remarks>
        /// <param name="rm">The WrapMode value to convert.</param>
        /// <returns>The HsmWrapMode value that corresponds to the specified WrapMode.</returns>
        public static HsmWrapMode ToHsmWrapMode(this WrapMode rm)
        {
            return rm switch
            {
                WrapMode.Once => HsmWrapMode.Once,
                WrapMode.Loop => HsmWrapMode.Loop,
                WrapMode.PingPong => HsmWrapMode.PingPong,
                WrapMode.Default => HsmWrapMode.Default,
                WrapMode.ClampForever => HsmWrapMode.Once,
                _ => throw new System.ArgumentOutOfRangeException(nameof(rm), $"Unexpected WrapMode value: {rm}"),
            };
        }

        /// <summary>
        /// Converts an HsmWrapMode value to the corresponding UnityEngine.WrapMode value.
        /// </summary>
        /// <param name="rm">The HsmWrapMode value to convert.</param>
        /// <returns>The equivalent UnityEngine.WrapMode value that corresponds to the specified HsmWrapMode.</returns>
        public static WrapMode ToUnityWrapMode(this HsmWrapMode rm)
        {
            return rm switch
            {
                HsmWrapMode.Once => WrapMode.Once,
                HsmWrapMode.Loop => WrapMode.Loop,
                HsmWrapMode.PingPong => WrapMode.PingPong,
                HsmWrapMode.Default => WrapMode.Default,
                _ => throw new System.ArgumentOutOfRangeException(nameof(rm), $"Unexpected HsmWrapMode value: {rm}"),
            };
        }
    }
}
