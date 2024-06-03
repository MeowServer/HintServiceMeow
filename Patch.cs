using HarmonyLib;
using Hints;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utf8Json;
using YamlDotNet.Core;
using Exiled.API.Features;

namespace HintServiceMeow
{
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    static class HintPatch
    {
        static bool Prefix(Hints.Hint hint, ref HintDisplay __instance)
        {
            try
            {
                var playerDisplay = PlayerDisplay.Get(Player.Get(__instance.connectionToClient));

                if (playerDisplay.UpdatedRecently())
                {
                    return true;
                }

                return false;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("ShowHint", typeof(string), typeof(float))]
    static class HintPatch2
    {
        static bool Prefix(string message, float duration, ref Player __instance)
        {
            return false;
        }
    }
}
