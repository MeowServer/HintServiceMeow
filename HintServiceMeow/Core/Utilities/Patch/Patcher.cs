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
            var methodInfo = typeof(HintDisplay).GetMethod(nameof(HintDisplay.Show));
            Harmony.Unpatch(methodInfo, HarmonyPatchType.All);
#if EXILED  
            var methodInfo2 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new Type[] { typeof(string), typeof(float) });
            var methodInfo3 = typeof(Exiled.API.Features.Player).GetMethod(nameof(Exiled.API.Features.Player.ShowHint), new Type[] { typeof(Exiled.API.Features.Hint) });

            Harmony.Unpatch(methodInfo2, HarmonyPatchType.All);
            Harmony.Unpatch(methodInfo3, HarmonyPatchType.All);
#endif

            // Patch the method
            Harmony.PatchAll();
        }

        public static void Unpatch()
        {
            Harmony?.UnpatchAll();
        }
    }
}
