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
    /// Used to adapt other plugins' hint system to HintServiceMeow's hint system
    /// </summary>
    internal class CompatibilityAdaptor
    {
        internal static readonly HashSet<string> RegisteredAssemblies = new HashSet<string>();
        private static readonly ConcurrentDictionary<string, IReadOnlyList<Hint>> HintCache = new ConcurrentDictionary<string, IReadOnlyList<Hint>>();

        private readonly ConcurrentDictionary<string, DateTime> _removeTime = new ConcurrentDictionary<string, DateTime>();
        private readonly HashSet<string> _suppressedAssemblies = new HashSet<string>();

        private readonly PlayerDisplay _playerDisplay;

        public CompatibilityAdaptor(PlayerDisplay playerDisplay)
        {
            this._playerDisplay = playerDisplay;
        }

        public void ShowHint(string assemblyName, string content, float timeToRemove)
        {
            RegisteredAssemblies.Add(assemblyName);

            if (Plugin.Config.DisabledCompatAdapter.Contains(assemblyName) //Config limitation
                || content.Length > ushort.MaxValue //Length limitation
                || _suppressedAssemblies.Contains(assemblyName)) //Rate limitation
                return;

            //Rate limitation
            _suppressedAssemblies.Add(assemblyName);
            Timing.CallDelayed(0.45f, () => _suppressedAssemblies.Remove(assemblyName));

            var internalAssemblyName = "CompatibilityAdaptor-" + assemblyName;

            //Remove after period of time
            _removeTime[internalAssemblyName] = DateTime.Now.AddSeconds(timeToRemove);
            Timing.CallDelayed(timeToRemove + 0.1f, () =>
            {
                if (DateTime.Now >= _removeTime[internalAssemblyName])
                    _playerDisplay.InternalClearHint(internalAssemblyName);
            });

            InternalShowHint(internalAssemblyName, content);
        }

        private async void InternalShowHint(string internalName, string content)
        {
            try
            {
                //Check if the hint is already cached
                if (HintCache.TryGetValue(content, out var cachedHintList))
                {
                    _playerDisplay.InternalClearHint(internalName, false);
                    _playerDisplay.InternalAddHint(internalName, cachedHintList);
                    return;
                }

                var startTime = DateTime.Now;

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
                        Log.Error($"Error while generating hint for {internalName}: {e}");
                    }

                    return new List<Hint>();
                });

                if (hintList is null || hintList.IsEmpty())
                    return;

                //Make sure for low performance server
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(0.45f))
                    return;

                //Cache
                HintCache[content] = new List<Hint>(hintList).AsReadOnly();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(15000);
                        HintCache.TryRemove(content, out _);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                });

                //Set up hint
                _playerDisplay.InternalClearHint(internalName, false);
                _playerDisplay.InternalAddHint(internalName, hintList);
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}