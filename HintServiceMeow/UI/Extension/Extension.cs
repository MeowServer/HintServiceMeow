using HintServiceMeow.UI.Utilities;

namespace HintServiceMeow.UI.Extension
{
    public static class ExiledPlayerExtension
    {
        public static PlayerUI GetPlayerUi(this Exiled.API.Features.Player player)
        {
            return PlayerUI.Get(player);
        }
    }

    public static class NWPlayerExtension
    {
        public static PlayerUI GetPlayerUi(this PluginAPI.Core.Player player)
        {
            return PlayerUI.Get(player);
        }
    }
}
