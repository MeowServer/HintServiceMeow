using System;
using System.Linq;
using PluginAPI.Core;
using System.Diagnostics.CodeAnalysis;

namespace HintServiceMeow.Integrations
{
#if EXILED
    //Not finished yet
    internal static class ScriptedEventsIntegration
    {
        public static Type APIHelper => Exiled.Loader.Loader.Plugins
            .FirstOrDefault(plugin => plugin.Assembly.GetName().Name == "ScriptedEvents")?
            .Assembly?
            .GetType("ScriptedEvents.API.Features.ApiHelper");

        public static void AddAction(string name, Func<string[], Tuple<bool, string>> action)
        {
            APIHelper.GetMethod("RegisterCustomAction")?
                .Invoke(null, new object[] { name, action });
        }

        public static void AddCustomActions()
        {
            AddAction("HSM_SHOWCOMMONHINT", ShowCommonHint);
        }

        public static Tuple<bool, string> ShowCommonHint(string[] arguments)
        {
            throw new NotImplementedException();
        }

        public static void UnregisterCustomActions()
        {
            APIHelper?.GetMethod("UnregisterCustomActions")?.Invoke(null, new object[] { new[] { "HSM_SHOWCOMMONHINT" } });
        }
    }
#endif
}
