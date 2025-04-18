﻿using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using System.Reflection;

namespace HintServiceMeow.Core.Extension
{
#if EXILED
    public static class ExiledPlayerExtension
    {
        public static PlayerDisplay GetPlayerDisplay(this Exiled.API.Features.Player player) => PlayerDisplay.Get(player);

        public static void AddHint(this Exiled.API.Features.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalAddHint(Assembly.GetCallingAssembly().FullName, hint);

        public static void RemoveHint(this Exiled.API.Features.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);
    }
#endif
    public static class NWPlayerExtension
    {
        public static PlayerDisplay GetPlayerDisplay(this PluginAPI.Core.Player player) => PlayerDisplay.Get(player);

        public static void AddHint(this PluginAPI.Core.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalAddHint(Assembly.GetCallingAssembly().FullName, hint);

        public static void RemoveHint(this PluginAPI.Core.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);
    }
}