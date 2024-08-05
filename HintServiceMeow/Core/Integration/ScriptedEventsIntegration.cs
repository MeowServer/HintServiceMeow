using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using HintServiceMeow.UI.Utilities;
using System.Security.Cryptography;

namespace HintServiceMeow.Core.Integration
{
    internal static class ScriptedEventsIntegration
    {
        private static Assembly ScriptedAssembly => Loader.Plugins.FirstOrDefault(plugin => plugin.Assembly.GetName().Name == "ScriptedEvents")?.Assembly;

        private static Type ApiHelper => ScriptedAssembly?.GetType("ScriptedEvents.API.Features.ApiHelper");

        public static void AddAllMethods()
        {
            if (ScriptedAssembly is null) return;

            void AddMethodToSE(string name, Func<string[], Tuple<bool, string>> action)
            {
                ApiHelper?.GetMethod("RegisterCustomAction").Invoke(null, new object[] { name, action });
            }

            AddMethodToSE("HSM_ItemHint", ShowItemHint);
            AddMethodToSE("HSM_MapHint", ShowMapHint);
            AddMethodToSE("HSM_RoleHint", ShowRoleHint);
            AddMethodToSE("HSM_OtherHint", ShowOtherHint);
        }

        public static void RemoveAllMethods()
        {
            ApiHelper?.GetMethod("UnregisterCustomActions").Invoke(null, new object[] { new[] { "RR_ADDREMARK", "RR_PAUSEREPORT" } });
        }

        private static IEnumerable<Player> GetPlayer(string variable, int max = -1)
        {
            return (IEnumerable<Player>)ApiHelper?.GetMethod("GetPlayers").Invoke(null, new object[] { variable, max });
        }

        private static Tuple<bool, string> ShowItemHint(string[] arguments)
        {
            if (arguments.Length < 2) return Tuple.Create(false, "Missing argument: ItemHint");

            var players = GetPlayer(arguments[0]);

            IEnumerable<string> contents;
            if (!float.TryParse(arguments[1], out float time))
            {
                contents = arguments.Skip(1);
            }
            else
            {
                contents = arguments.Skip(2);
            }

            foreach (var player in players)
            {
                if (time == 0)
                {
                    PlayerUI.Get(player).CommonHint.ShowItemHint(contents.FirstOrDefault() ?? "",
                        contents.Skip(1).ToArray(), default);
                }
                else
                {
                    PlayerUI.Get(player).CommonHint
                        .ShowItemHint(contents.FirstOrDefault() ?? "", contents.Skip(1).ToArray(), time);
                }
            }

            return Tuple.Create(true, string.Empty);
        }

        private static Tuple<bool, string> ShowMapHint(string[] arguments)
        {
            if (arguments.Length < 2) return Tuple.Create(false, "Missing argument: ItemHint");

            var players = GetPlayer(arguments[0]);

            IEnumerable<string> contents;
            if (!float.TryParse(arguments[1], out float time))
            {
                contents = arguments.Skip(1);
            }
            else
            {
                contents = arguments.Skip(2);
            }

            foreach (var player in players)
            {
                if (time == 0)
                {
                    PlayerUI.Get(player).CommonHint.ShowMapHint(contents.FirstOrDefault() ?? "",
                        contents.Skip(1).ToArray(), default);
                }
                else
                {
                    PlayerUI.Get(player).CommonHint
                        .ShowMapHint(contents.FirstOrDefault() ?? "", contents.Skip(1).ToArray(), time);
                }
            }

            return Tuple.Create(true, string.Empty);
        }

        private static Tuple<bool, string> ShowRoleHint(string[] arguments)
        {
            if (arguments.Length < 2) return Tuple.Create(false, "Missing argument: ItemHint");

            var players = GetPlayer(arguments[0]);

            IEnumerable<string> contents;
            if (!float.TryParse(arguments[1], out float time))
            {
                contents = arguments.Skip(1);
            }
            else
            {
                contents = arguments.Skip(2);
            }

            foreach (var player in players)
            {
                if (time == 0)
                {
                    PlayerUI.Get(player).CommonHint.ShowRoleHint(contents.FirstOrDefault() ?? "",
                        contents.Skip(1).ToArray(), default);
                }
                else
                {
                    PlayerUI.Get(player).CommonHint
                        .ShowRoleHint(contents.FirstOrDefault() ?? "", contents.Skip(1).ToArray(), time);
                }
            }

            return Tuple.Create(true, string.Empty);
        }

        private static Tuple<bool, string> ShowOtherHint(string[] arguments)
        {
            if (arguments.Length < 2) return Tuple.Create(false, "Missing argument: ItemHint");

            var players = GetPlayer(arguments[0]);
            
            IEnumerable<string> contents;
            if (!float.TryParse(arguments[1], out float time))
            {
                contents = arguments.Skip(1);
            }
            else
            {
                contents = arguments.Skip(2);
            }

            foreach (var player in players)
            {
                if (time == 0)
                {
                    PlayerUI.Get(player).CommonHint.ShowOtherHint(contents.FirstOrDefault() ?? "", default);
                }
                else
                {
                    PlayerUI.Get(player).CommonHint.ShowOtherHint(contents.FirstOrDefault() ?? "", time);
                }
            }

            return Tuple.Create(true, string.Empty);
        }
    }
}
