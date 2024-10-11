using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;

using MEC;
using PluginAPI.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HintServiceMeow.Core.Utilities.Pools;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Used to adapt other plugins' hint system to HintServiceMeow's hint system
    /// </summary>
    internal class CompatibilityAdaptor
    {
        internal static readonly HashSet<string> RegisteredAssemblies = new HashSet<string>();
        private static readonly ConcurrentDictionary<string, IReadOnlyList<Hint>> HintCache = new ConcurrentDictionary<string, IReadOnlyList<Hint>>();

        private readonly ConcurrentDictionary<string, CoroutineHandle> _removeDelayedActions = new ConcurrentDictionary<string, CoroutineHandle>();
        private readonly HashSet<string> _suppressedAssemblies = new HashSet<string>();

        private readonly PlayerDisplay _playerDisplay;

        public CompatibilityAdaptor(PlayerDisplay playerDisplay)
        {
            this._playerDisplay = playerDisplay;
        }

        public void ShowHint(string assemblyName, string content, float duration)
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

            //Stop previous remove action
            if (_removeDelayedActions.TryGetValue(internalAssemblyName, out var removeTime) && removeTime.IsRunning)
                Timing.KillCoroutines(removeTime);

            //Check duration, if duration is less than 0, then only clear the hints but don't generate new hints.
            if (duration <= 0)
            {
                _playerDisplay.InternalClearHint(internalAssemblyName);
                return;
            }

            if(duration > float.MaxValue - 0.1f)
                duration = float.MaxValue - 0.1f; //Prevent overflow (Max value is 0.1f less than float.MaxValue)

            //Start new remove action, remove after the duration
            _removeDelayedActions[internalAssemblyName] = Timing.CallDelayed(duration + 0.1f, () => _playerDisplay.InternalClearHint(internalAssemblyName));

            InternalShowHint(internalAssemblyName, content, DateTime.Now.AddSeconds(duration));
        }

        private async void InternalShowHint(string internalAssemblyName, string content, DateTime expireTime)
        {
            try
            {
                //Check if the hint is already cached
                if (HintCache.TryGetValue(content, out var cachedHintList))
                {
                    _playerDisplay.InternalClearHint(internalAssemblyName, false);
                    _playerDisplay.InternalAddHint(internalAssemblyName, cachedHintList);

                    return;
                }

                var startTime = DateTime.Now;

                //If not cached, then generate hint
                List<Hint> hintList = await Task.Run(() =>
                {
                    try
                    {
                        var lineInfoList = RichTextParserPool.ParseText(content, 40);

                        if (lineInfoList is null || lineInfoList.IsEmpty())
                            return new List<Hint>();

                        var totalHeight = lineInfoList.Sum(x => x.Height);
                        var accumulatedHeight = 0f;
                        List<Hint> generatedHintList = new List<Hint>();

                        foreach (var lineInfo in lineInfoList)
                        {
                            //If not empty line, then add hint
                            if (!string.IsNullOrEmpty(lineInfo.RawText.Trim()) && !lineInfo.Characters.IsEmpty())
                            {
                                generatedHintList.Add(new Hint
                                {
                                    Text = lineInfo.RawText,
                                    XCoordinate = lineInfo.Pos,
                                    YCoordinate = 700 - totalHeight / 2 + lineInfo.Height + accumulatedHeight,
                                    YCoordinateAlign = HintVerticalAlign.Bottom,
                                    Alignment = lineInfo.Alignment,
                                    FontSize = (int)lineInfo.Characters.First().FontSize,
                                });
                            }  

                            accumulatedHeight += lineInfo.Height;
                        }

                        return generatedHintList;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error while generating hint for {internalAssemblyName}: {e}");
                        return new List<Hint>();
                    }
                });

                if (hintList is null || hintList.IsEmpty())
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

                //Make sure for low performance server or if the duration is shorter than converting time.
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(0.45f) || DateTime.Now > expireTime)
                    return;

                //Set up hint
                _playerDisplay.InternalClearHint(internalAssemblyName, false);
                _playerDisplay.InternalAddHint(internalAssemblyName, hintList);
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}