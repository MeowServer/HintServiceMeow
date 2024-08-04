using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Enum;
//Plugin API
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
//Exiled
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;

namespace HintServiceMeow
{
    internal static class ExiledEventHandler
    {
        // TryCreate PlayerDisplay and PlayerUIConfig for the new ReferenceHub
        internal static void OnVerified(VerifiedEventArgs ev)
        {
            if(ev.Player is null)
                return;

            if (ev.Player.IsNPC || ev.Player.ReferenceHub.isLocalPlayer) 
                return;

            PlayerDisplay.TryCreate(ev.Player.ReferenceHub);
            PlayerUI.TryCreate(ev.Player.ReferenceHub);
        }

        internal static void OnLeft(LeftEventArgs ev)
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Destruct(ev.Player.ReferenceHub);
        }
    }

    internal class NwapiEventHandler
    {
        [PluginEvent(ServerEventType.PlayerJoined)]
        internal void OnJoin(PlayerJoinedEvent ev)
        {
            if (ev.Player is null || ev.Player.ReferenceHub.isLocalPlayer || ev.Player.UserId is null)
                return;

            PlayerDisplay.TryCreate(ev.Player.ReferenceHub);
            PlayerUI.TryCreate(ev.Player.ReferenceHub);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        internal void OnLeft(PlayerLeftEvent ev)
        {
            PlayerUI.Destruct(ev.Player.ReferenceHub);
            PlayerDisplay.Destruct(ev.Player.ReferenceHub);
        }
    }
}
