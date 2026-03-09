using HintServiceMeow.Core.Enum.UnityAdaptor;
using UnityEngine;

namespace HintServiceMeow.Core.Extension
{
    public static class WrapModeExtension
    {
        public static HsmWrapMode ToHsmWrapMode(this WrapMode rm)
        {
            return rm switch
            {
                WrapMode.Once => HsmWrapMode.Once,
                WrapMode.Loop => HsmWrapMode.Loop,
                WrapMode.PingPong => HsmWrapMode.PingPong,
                WrapMode.Default => HsmWrapMode.Default,
                WrapMode.ClampForever => HsmWrapMode.ClampForever,
            };
        }

        public static WrapMode ToUnityWarpMode(this HsmWrapMode rm)
        {
            return rm switch
            {
                HsmWrapMode.Once => WrapMode.Once,
                HsmWrapMode.Loop => WrapMode.Loop,
                HsmWrapMode.PingPong => WrapMode.PingPong,
                HsmWrapMode.Default => WrapMode.Default,
                HsmWrapMode.ClampForever => WrapMode.ClampForever,
            };
        }
    }
}
