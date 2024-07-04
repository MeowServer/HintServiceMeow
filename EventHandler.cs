using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow
{
    public static class EventHandler
    {
        public delegate void NewPlayerHandler(PlayerDisplay playerDisplay);
        public static event NewPlayerHandler NewPlayer;

        internal static void InvokeNewPlayerEvent(PlayerDisplay pd)
        {
            NewPlayer?.Invoke(pd);
        }

        // Create PlayerDisplay and PlayerUIConfig for the new player
        internal static void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNPC) return;

            var pd = new PlayerDisplay(ev.Player);
            new PlayerUI(ev.Player);

            EventHandler.InvokeNewPlayerEvent(pd);
        }

        internal static void OnLeft(LeftEventArgs ev)
        {
            PlayerUI.RemovePlayerUI(ev.Player);
            PlayerDisplay.RemovePlayerDisplay(ev.Player);
        }

        internal static void OnDisable()
        {
            PlayerUI.ClearPlayerUI();
            PlayerDisplay.ClearPlayerDisplay();
        }
    }
}
