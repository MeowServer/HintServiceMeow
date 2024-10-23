using System;
using System.Reflection;
using Hints;
using HintServiceMeow.Core.Extension;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Utilities.Patch
{
    internal static class Patches
    {
        public static bool HintDisplayPatch(ref Hint hint, ref HintDisplay __instance)
        {
            try
            {
                if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                    return false;

                if (hint is TextHint textHint && ReferenceHub.TryGetHubNetID(__instance.connectionToClient.identity.netId, out var referenceHub))
                {
                    var assemblyName = Assembly.GetCallingAssembly().FullName;
                    var content = textHint.Text;
                    var duration = textHint.DurationScalar;
                    PlayerDisplay.Get(referenceHub).ShowCompatibilityHint(assemblyName, content, duration);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }

        public static bool ReceiveHintPatch1(ref string text, ref float duration, ref Player __instance)
        {
            try
            {
                if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                    return false;
            
                var assemblyName = Assembly.GetCallingAssembly().FullName;
                __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, text, duration);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }

        public static bool ReceiveHintPatch2(ref string text, ref HintEffect[] effects, ref float duration, ref Player __instance)
        {
            try
            {
                if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                    return false;

                var assemblyName = Assembly.GetCallingAssembly().FullName;
                __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, text, duration);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }

#if EXILED
        public static bool ExiledHintPatch1(ref string message, ref float duration, ref Exiled.API.Features.Player __instance)
        {
            try
            {
                if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                    return false;

                var assemblyName = Assembly.GetCallingAssembly().FullName;
                __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, message, duration);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }

        public static bool ExiledHintPatch2(ref Exiled.API.Features.Hint hint, ref Exiled.API.Features.Player __instance)
        {
            try
            {
                if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                    return false;

                if (!hint.Show)
                    return false;
                
                var assemblyName = Assembly.GetCallingAssembly().FullName;
                __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, hint.Content, hint.Duration);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }
#endif
    }
}
