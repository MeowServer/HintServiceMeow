using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hints;
using PluginAPI.Core;
using Hint = Hints.Hint;

namespace HintServiceMeow.Core.Utilities.Patch
{
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    internal static class HintDisplayPatch
    {
        private static bool Prefix(ref Hint hint, ref HintDisplay __instance)
        {
            if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                return false;

            try
            {
                if (hint is TextHint textHint)
                    if(ReferenceHub.TryGetHubNetID(__instance.connectionToClient.identity.netId, out var referenceHub))
                    {
                        var assemblyName = Assembly.GetCallingAssembly().GetName().Name;
                        var content = textHint.Text;
                        var timeToRemove = textHint.DurationScalar;

                        CompatibilityAdapter.ShowHint(referenceHub, assemblyName, content, timeToRemove);
                    }
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }
    }

#if EXILED
    [HarmonyPatch(typeof(Exiled.API.Features.Player), nameof(Exiled.API.Features.Player.ShowHint))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    internal static class ExiledHintPatch
    {
        private static bool Prefix(ref string message, ref float duration, ref Exiled.API.Features.Player __instance)
        {
            if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                return false;

            try
            {
                var assemblyName = Assembly.GetCallingAssembly().GetName().Name;

                CompatibilityAdapter.ShowHint(__instance.ReferenceHub, assemblyName, message, duration);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }
    }
#endif
}
