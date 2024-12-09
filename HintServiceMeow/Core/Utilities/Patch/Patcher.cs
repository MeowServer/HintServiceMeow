using HarmonyLib;
using System;

namespace HintServiceMeow.Core.Utilities.Patch
{
    internal static class Patcher
    {
        private static Harmony Harmony { get; set; }

        public static void Patch()
        {
            Harmony = new Harmony("HintServiceMeowHarmony" + Guid.NewGuid());

            //Unpatch all other patches
            var hintDisplayMethod = typeof(Hints.HintDisplay).GetMethod(nameof(Hints.HintDisplay.Show));
            var receiveHintMethod1 = typeof(PluginAPI.Core.Player).GetMethod(nameof(PluginAPI.Core.Player.ReceiveHint), new[] { typeof(string), typeof(float) });
            var receiveHintMethod2 = typeof(PluginAPI.Core.Player).GetMethod(nameof(PluginAPI.Core.Player.ReceiveHint), new[] { typeof(string), typeof(Hints.HintEffect[]), typeof(float) });
            Harmony.Unpatch(hintDisplayMethod, HarmonyPatchType.All);
            Harmony.Unpatch(receiveHintMethod1, HarmonyPatchType.All);
            Harmony.Unpatch(receiveHintMethod2, HarmonyPatchType.All);

            var patchType = typeof(Patches);

            // Patch the method
            Harmony.Patch(hintDisplayMethod, new HarmonyMethod(patchType.GetMethod(nameof(Patches.HintDisplayPatch))));
            Harmony.Patch(receiveHintMethod1, new HarmonyMethod(patchType.GetMethod(nameof(Patches.ReceiveHintPatch1))));
            Harmony.Patch(receiveHintMethod2, new HarmonyMethod(patchType.GetMethod(nameof(Patches.ReceiveHintPatch2))));


#if EXILED
            //Exiled methods
            var showHintMethod1 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new[] { typeof(string), typeof(float) });
            var showHintMethod2 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new[] { typeof(Exiled.API.Features.Hint) });

            Harmony.Unpatch(showHintMethod1, HarmonyPatchType.All);
            Harmony.Unpatch(showHintMethod2, HarmonyPatchType.All);

            var exiledHintPatch1 = patchType.GetMethod(nameof(Patches.ExiledHintPatch1));
            var exiledHintPatch2 = patchType.GetMethod(nameof(Patches.ExiledHintPatch2));
            Harmony.Patch(showHintMethod1, new HarmonyMethod(exiledHintPatch1));
            Harmony.Patch(showHintMethod2, new HarmonyMethod(exiledHintPatch2));
#endif
        }

        public static void Unpatch()
        {
            Harmony?.UnpatchAll();
        }
    }
}
