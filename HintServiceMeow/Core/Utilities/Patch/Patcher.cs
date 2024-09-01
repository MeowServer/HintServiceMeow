using HarmonyLib;
using Hints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities.Patch
{
    internal static class Patcher
    {
        private static Harmony Harmony { get; set; }

        public static void Patch()
        {
            Harmony = new Harmony("HintServiceMeowHarmony" + Plugin.Version);

            //Unpatch all other patches
            var hintDisplayMethod = typeof(HintDisplay).GetMethod(nameof(HintDisplay.Show));
            var receiveHintMethod1 = typeof(PluginAPI.Core.Player).GetMethod(nameof(PluginAPI.Core.Player.ReceiveHint), new Type[] { typeof(string), typeof(float) });
            var receiveHintMethod2 = typeof(PluginAPI.Core.Player).GetMethod(nameof(PluginAPI.Core.Player.ReceiveHint), new Type[] { typeof(string), typeof(HintEffect[]), typeof(float) });
            Harmony.Unpatch(hintDisplayMethod, HarmonyPatchType.All);
            Harmony.Unpatch(receiveHintMethod1, HarmonyPatchType.All);
            Harmony.Unpatch(receiveHintMethod2, HarmonyPatchType.All);

            // Patch the method
            var hintDisplayPatch = typeof(HintDisplayPatch).GetMethod(nameof(HintDisplayPatch.Prefix));
            var receiveHintPatch = typeof(NWAPIHintPatch).GetMethod(nameof(NWAPIHintPatch.Prefix));

            Harmony.Patch(hintDisplayMethod, new HarmonyMethod(hintDisplayPatch));
            Harmony.Patch(receiveHintMethod1, new HarmonyMethod(receiveHintPatch));
            Harmony.Patch(receiveHintMethod2, new HarmonyMethod(receiveHintPatch));

            //Exiled methods
#if EXILED
            var showHintMethod1 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new Type[] { typeof(string), typeof(float) });
            var showHintMethod2 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new Type[] { typeof(Exiled.API.Features.Hint) });

            Harmony.Unpatch(showHintMethod1, HarmonyPatchType.All);
            Harmony.Unpatch(showHintMethod2, HarmonyPatchType.All);

            var exiledHintPatch = typeof(ExiledHintPatch).GetMethod(nameof(ExiledHintPatch.Prefix));
            Harmony.Patch(showHintMethod1, new HarmonyMethod(exiledHintPatch));
#endif
        }

        public static void Unpatch()
        {
            Harmony?.UnpatchAll();
        }
    }
}
