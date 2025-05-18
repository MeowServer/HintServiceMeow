using HarmonyLib;
using System;
using System.Reflection;

namespace HintServiceMeow.Core.Utilities.Patch
{
    public static class Patcher
    {
        public static Harmony Harmony { get; private set; }

        internal static void Patch()
        {
            Harmony = new Harmony("HintServiceMeowHarmony" + Guid.NewGuid());

            //Unpatch all other patches
            MethodInfo hintDisplayMethod = typeof(Hints.HintDisplay).GetMethod(nameof(Hints.HintDisplay.Show));
            MethodInfo sendHintMethod1 = typeof(LabApi.Features.Wrappers.Player).GetMethod(nameof(LabApi.Features.Wrappers.Player.SendHint), new[] { typeof(string), typeof(float) });
            MethodInfo sendHintMethod2 = typeof(LabApi.Features.Wrappers.Player).GetMethod(nameof(LabApi.Features.Wrappers.Player.SendHint), new[] { typeof(string), typeof(Hints.HintEffect[]), typeof(float) });
            Harmony.Unpatch(hintDisplayMethod, HarmonyPatchType.All);
            Harmony.Unpatch(sendHintMethod1, HarmonyPatchType.All);
            Harmony.Unpatch(sendHintMethod2, HarmonyPatchType.All);

            Type patchType = typeof(Patches);

            // Patch the method
            Harmony.Patch(hintDisplayMethod, new HarmonyMethod(patchType.GetMethod(nameof(Patches.HintDisplayPatch))));
            Harmony.Patch(sendHintMethod1, new HarmonyMethod(patchType.GetMethod(nameof(Patches.SendHintPatch1))));
            Harmony.Patch(sendHintMethod2, new HarmonyMethod(patchType.GetMethod(nameof(Patches.SendHintPatch2))));


#if EXILED
            //Exiled methods
            MethodInfo showHintMethod1 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new[] { typeof(string), typeof(float) });
            MethodInfo showHintMethod2 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new[] { typeof(Exiled.API.Features.Hint) });

            Harmony.Unpatch(showHintMethod1, HarmonyPatchType.All);
            Harmony.Unpatch(showHintMethod2, HarmonyPatchType.All);

            MethodInfo exiledHintPatch1 = patchType.GetMethod(nameof(Patches.ExiledHintPatch1));
            MethodInfo exiledHintPatch2 = patchType.GetMethod(nameof(Patches.ExiledHintPatch2));
            Harmony.Patch(showHintMethod1, new HarmonyMethod(exiledHintPatch1));
            Harmony.Patch(showHintMethod2, new HarmonyMethod(exiledHintPatch2));
#endif
        }

        internal static void Unpatch()
        {
            Harmony?.UnpatchAll();
        }
    }
}
