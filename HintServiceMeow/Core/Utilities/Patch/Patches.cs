using System;
using System.Reflection;
using Hints;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Utilities.Patch
{
    internal static class Patches
    {
        public static bool HintDisplayPatch(ref Hint hint, ref HintDisplay __instance)
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

                        CompatibilityAdaptor.ShowHint(referenceHub, assemblyName, content, timeToRemove);
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
            if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                return false;

            try
            {
                var assemblyName = Assembly.GetCallingAssembly().GetName().Name;

                CompatibilityAdaptor.ShowHint(__instance.ReferenceHub, assemblyName, text, duration);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }

        public static bool ReceiveHintPatch2(ref string text, ref HintEffect[] effects, ref float duration, ref Player __instance)
        {
            if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                return false;

            try
            {
                var assemblyName = Assembly.GetCallingAssembly().GetName().Name;

                CompatibilityAdaptor.ShowHint(__instance.ReferenceHub, assemblyName, text, duration);
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
            if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                return false;

            try
            {
                var assemblyName = Assembly.GetCallingAssembly().GetName().Name;

                CompatibilityAdaptor.ShowHint(__instance.ReferenceHub, assemblyName, message, duration);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }

        public static bool ExiledHintPatch2(ref Exiled.API.Features.Hint hint, ref Exiled.API.Features.Player __instance)
        {
            if (!PluginConfig.Instance.UseHintCompatibilityAdapter)
                return false;

            if (!hint.Show)
                return false;

            try
            {
                var assemblyName = Assembly.GetCallingAssembly().GetName().Name;

                CompatibilityAdaptor.ShowHint(__instance.ReferenceHub, assemblyName, hint.Content, hint.Duration);
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
