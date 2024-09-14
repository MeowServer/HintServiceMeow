using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Tools;
using PluginAPI.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Compatibility adaptor design to adapt other plugins' hint system to HintServiceMeow's hint system
    /// </summary>
    internal static class CompatibilityAdaptor
    {
        internal static readonly HashSet<string> RegisteredAssemblies = new HashSet<string>(); //Include all the assembly names that used this adaptor
        internal static readonly object RegisteredAssembliesLock = new object();

        private static readonly ConcurrentDictionary<string, DateTime> RemoveTime = new ConcurrentDictionary<string, DateTime>();

        private static readonly ConcurrentDictionary<string, List<Hint>> HintCache = new ConcurrentDictionary<string, List<Hint>>();

        private static readonly ConcurrentDictionary<string, CancellationTokenSource> CancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static void ShowHint(ReferenceHub player, string assemblyName, string content, float timeToRemove)
        {
            if (Plugin.Config.DisabledCompatAdapter.Contains(assemblyName))
                return;

            lock(RegisteredAssembliesLock)
                RegisteredAssemblies.Add(assemblyName);

            if (CancellationTokens.TryGetValue(assemblyName, out var token))
            {
                token.Cancel();
                token.Dispose();
            }

            var cancellationTokenSource = new CancellationTokenSource();
            CancellationTokens[assemblyName] = cancellationTokenSource;

            _ = InternalShowHint(PlayerDisplay.Get(player), assemblyName, content, timeToRemove, cancellationTokenSource.Token);
        }

        public static async Task InternalShowHint(PlayerDisplay playerDisplay, string assemblyName, string content, float timeToRemove, CancellationToken cancellationToken)
        {
            assemblyName = "CompatibilityAdaptor-" + assemblyName;

            if (playerDisplay == null)
                return;

            //Reset the time to remove
            RemoveTime[assemblyName] = DateTime.Now.AddSeconds(timeToRemove);

            //Check if the hint is already cached
            if(HintCache.TryGetValue(content, out var cachedHintList))
            {
                playerDisplay.InternalClearHint(assemblyName);
                playerDisplay.InternalAddHint(assemblyName, cachedHintList);

                //Remove after time to remove
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(timeToRemove + 0.2f), cancellationToken);

                    if (!cancellationToken.IsCancellationRequested && RemoveTime[assemblyName] <= DateTime.Now)
                        playerDisplay.InternalClearHint(assemblyName);
                }
                catch (TaskCanceledException) { }

                return;
            }

            //If no cache, then generate hint
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

                    var textList = content.Split('\n');

                    List<Hint> generatedHintList = new List<Hint>();
                    foreach(var lineInfo in lineInfoList)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return generatedHintList;

                        //If not empty line, then add hint
                        if (!string.IsNullOrEmpty(lineInfo.RawText.Trim()) && !lineInfo.Characters.IsEmpty())
                            generatedHintList.Add(new CompatAdapterHint
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
                catch(Exception e)
                {
                    Log.Error($"Error while generating hint for {assemblyName}: {e}");
                }

                return new List<Hint>();
            }, cancellationToken);

            if(hintList.IsEmpty() || cancellationToken.IsCancellationRequested)
                return;

            //Set hint
            playerDisplay.InternalClearHint(assemblyName);
            playerDisplay.InternalAddHint(assemblyName, hintList);

            //Remove hint after time to remove
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeToRemove + 0.2f), cancellationToken);

                if (RemoveTime[assemblyName] <= DateTime.Now)
                    playerDisplay.InternalClearHint(assemblyName);
            }
            catch (TaskCanceledException) { }

            //Add generated hint into cache
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(15000, cancellationToken);
                    HintCache.TryRemove(content, out var _);
                }
                catch (TaskCanceledException) { }
            }, cancellationToken);
        }
    }
}