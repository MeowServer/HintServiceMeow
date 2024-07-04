using HarmonyLib;
using Hints;
using System;
using Exiled.API.Features;

namespace HintServiceMeow
{
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    internal static class HintPatch
    {
        static bool Prefix(Hints.Hint hint, ref HintDisplay __instance)
        {
            try
            {
                var playerDisplay = PlayerDisplay.Get(Player.Get(__instance.connectionToClient));
                return playerDisplay.AllowPatchUpdate;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("ShowHint", typeof(string), typeof(float))]
    internal static class HintPatch2
    {
        static bool Prefix(string message, float duration, ref Player __instance)
        {
            return false;
        }
    }
}
