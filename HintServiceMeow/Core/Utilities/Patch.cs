using System;
using HarmonyLib;
using Hints;
using PluginAPI.Core;
using Hint = Hints.Hint;

namespace HintServiceMeow.Core.Utilities
{
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    internal static class HintDisplayPatch
    {
        static bool Prefix(ref Hint hint, ref HintDisplay __instance)
        {
            return false;
        }
    }
}
