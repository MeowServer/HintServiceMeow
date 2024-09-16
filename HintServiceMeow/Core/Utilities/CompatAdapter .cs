using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using MEC;
using PluginAPI.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Compatibility adaptor design to adapt other plugins' hint system to HintServiceMeow's hint system
    /// </summary>
    internal static class CompatibilityAdaptor
    {
        internal static readonly HashSet<string> RegisteredAssemblies = new HashSet<string>(); //Include all the assembly names that used this adaptor
        internal static readonly object RegisteredAssembliesLock = new object();

        private static readonly ConcurrentDictionary<string, IReadOnlyList<Hint>> HintCache = new ConcurrentDictionary<string, IReadOnlyList<Hint>>();

        private static readonly ConcurrentDictionary<string, DateTime> RemoveTime = new ConcurrentDictionary<string, DateTime>();

        private static readonly HashSet<string> SuppressedAssemblies = new HashSet<string>();

        public static void ShowHint(ReferenceHub player, string assemblyName, string content, float timeToRemove)
        {
            lock (RegisteredAssembliesLock)
                RegisteredAssemblies.Add(assemblyName);

            if (Plugin.Config.DisabledCompatAdapter.Contains(assemblyName) //Config limitation
                || content.Length > ushort.MaxValue //Length limitation
                || SuppressedAssemblies.Contains(assemblyName)) //Rate limitation
                return;

            //Rate limitation
            SuppressedAssemblies.Add(assemblyName);
            Timing.CallDelayed(0.45f, () => RegisteredAssemblies.Remove(assemblyName));

            assemblyName = "CompatibilityAdaptor-" + assemblyName;
            var playerDisplay = PlayerDisplay.Get(player);

            _ = Task.Run(() => InternalShowHint(playerDisplay, assemblyName, content, timeToRemove));
        }

        public static async void InternalShowHint(PlayerDisplay playerDisplay, string assemblyName, string content, float timeToRemove)
        {
            if (playerDisplay is null)
                return;

            try
            {
                RemoveTime[assemblyName] = DateTime.Now + TimeSpan.FromSeconds(timeToRemove);

                var startTime = DateTime.Now;

                //Check if the hint is already cached
                if (HintCache.TryGetValue(content, out var cachedHintList))
                {
                    playerDisplay.InternalClearHint(assemblyName);
                    playerDisplay.InternalAddHint(assemblyName, cachedHintList);

                    //Remove after hint expire
                    await Task.Delay(TimeSpan.FromSeconds(timeToRemove + 0.05f));

                    if(RemoveTime.TryGetValue(assemblyName, out var removeTime) && removeTime < DateTime.Now)
                        playerDisplay.InternalClearHint(assemblyName);

                    return;
                }

                //If not cached, then generate hint
                List<Hint> hintList = await Task.Run(() =>
                {
                    try
                    {
                        var parser = new RichTextParser();

                        var lineInfoList = parser.ParseText(content, 40);

                        if (lineInfoList is null || lineInfoList.IsEmpty())
                            return new List<Hint>();

                        var totalHeight = lineInfoList.Sum(x => x.Height);
                        var accumulatedHeight = 0f;

                        List<Hint> generatedHintList = new List<Hint>();
                        foreach (var lineInfo in lineInfoList)
                        {
                            //If not empty line, then add hint
                            if (!string.IsNullOrEmpty(lineInfo.RawText.Trim()) && !lineInfo.Characters.IsEmpty())
                                generatedHintList.Add(new Hint
                                {
                                    Text = lineInfo.RawText,
                                    XCoordinate = lineInfo.Pos,
                                    YCoordinate = 700 - totalHeight / 2 + lineInfo.Height + accumulatedHeight,
                                    YCoordinateAlign = HintVerticalAlign.Bottom,
                                    Alignment = lineInfo.Alignment,
                                    FontSize = (int)lineInfo.Characters.First().FontSize,
                                });

                            accumulatedHeight += lineInfo.Height;
                        }

                        return generatedHintList;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error while generating hint for {assemblyName}: {e}");
                    }

                    return new List<Hint>();
                });

                if (hintList is null || hintList.IsEmpty())
                    return;

                //Make sure for low performance server
                var elapsed = DateTime.Now - startTime;
                if (elapsed > TimeSpan.FromSeconds(0.5) || elapsed > TimeSpan.FromSeconds(timeToRemove))
                    return;

                //Set up hint
                playerDisplay.InternalClearHint(assemblyName);
                playerDisplay.InternalAddHint(assemblyName, hintList);

                //Remove after hint expire
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(timeToRemove + 0.05f));

                        if (RemoveTime.TryGetValue(assemblyName, out var removeTime) && removeTime < DateTime.Now)
                            playerDisplay.InternalClearHint(assemblyName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                });

                //Cache
                HintCache[content] = new List<Hint>(hintList).AsReadOnly();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(15000);
                        HintCache.TryRemove(content, out var _);
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                });
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}