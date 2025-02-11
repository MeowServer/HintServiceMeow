using HintServiceMeow.UI.Utilities;

namespace HintServiceMeow.UI.Extension
{
#if EXILED
    public static class ExiledPlayerExtension
    {
        public static PlayerUI GetPlayerUi(this Exiled.API.Features.Player player)
        {
            return PlayerUI.Get(player);
        }
    }
#endif

    public static class NWPlayerExtension
    {
        public static PlayerUI GetPlayerUi(this LabApi.Features.Wrappers.Player player)
        {
            return PlayerUI.Get(player.ReferenceHub);
        }
    }
}
