﻿using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Core.Utilities.Tools;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Used to adapt other plugins' hint system to HintServiceMeow's hint system
    /// </summary>
    internal class CompatibilityAdaptor : ICompatibilityAdaptor, Interface.IDestructible
    {
        private static readonly ICache<string, IReadOnlyList<Hint>> HintCache = new Cache<string, IReadOnlyList<Hint>>(500);

        private bool _destructed = false; // To prevent multiple destruct calls

        private readonly Dictionary<string, CoroutineHandle> RemoveHandles = new();
        private readonly PlayerDisplay _playerDisplay;
        private readonly IPool<RichTextParser> _richTextParserPool;

        internal CompatibilityAdaptor(
            PlayerDisplay playerDisplay,
            IPool<RichTextParser> richTextParserPool = null
            )
        {
            this._playerDisplay = playerDisplay ?? throw new ArgumentNullException(nameof(playerDisplay));
            this._richTextParserPool = richTextParserPool ?? RichTextParserPool.Instance;
        }

        void Interface.IDestructible.Destruct()
        {
            if (_destructed)
                return;

            foreach (var handle in RemoveHandles.Values)
            {
                Timing.KillCoroutines(handle); // Stop all running coroutines
            }

            RemoveHandles.Clear(); // Clear the dictionary

            _destructed = true; // Mark as destructed
        }

        public void ShowHint(CompatibilityAdaptorArg ev)
        {
            if (ev is null)
                throw new ArgumentNullException(nameof(ev));

            string assemblyName = ev.AssemblyName;
            string content = ev.Content ?? string.Empty;
            float duration = Math.Min(ev.Duration, float.MaxValue - 1f);

            // Record the assembly that is using the compatibility adaptor
            GetCompatAssemblyName.RegisteredAssemblies.Add(assemblyName);

            if (PluginConfig.Instance.DisabledCompatAdapter.Contains(assemblyName) //Config limitation
                || content.Length > ushort.MaxValue) //Length limitation
                return;

            // Use internal assembly name to ensure safety
            string internalAssemblyName = "CompatibilityAdaptor-" + assemblyName;

            //For negative duration or empty content, clear hint
            if (duration <= 0f || string.IsNullOrEmpty(content))
            {
                _playerDisplay.InternalClearHint(internalAssemblyName);
                _playerDisplay.ForceUpdate();
                return;
            }

            // Clear hint after duration + 0.1 seconds
            if (_destructed) // If the adaptor has been destructed, do not proceed
                return;

            if (RemoveHandles.TryGetValue(internalAssemblyName, out var oldHandle))
                Timing.KillCoroutines(oldHandle); // Stop the previous coroutine if exists

            // Start a new coroutine to remove the hint after the duration
            RemoveHandles[internalAssemblyName] =
                Timing.CallDelayed(duration + 0.1f, () =>
                {
                    _playerDisplay.InternalClearHint(internalAssemblyName);
                    _playerDisplay.ForceUpdate();
                    RemoveHandles.Remove(internalAssemblyName);
                });

            DateTime expireTime = DateTime.Now.AddSeconds(Math.Min(duration, 5f)); // Wait for at most 5 second and at least the duration

            //Start new remove action, remove after the Duration
            _ = InternalShowHint(internalAssemblyName, content, expireTime);
        }

        private async Task InternalShowHint(string internalAssemblyName, string content, DateTime expireTime)
        {
            try
            {
                //Check if the hint is already cached
                if (HintCache.TryGet(content, out IReadOnlyList<Hint> cachedHintList))
                {
                    ReplaceHint(internalAssemblyName, cachedHintList);
                    return;
                }

                //Parse the content to hints
                IReadOnlyList<Hint> hintList = await ConcurrentTaskDispatcher.Instance.
                    Enqueue(async () => this.ParseRichTextToHints(content))
                    .ConfigureAwait(false);

                //Add result to cache
                HintCache.Add(content, hintList);

                // Update if the content is not outdated
                if (DateTime.Now < expireTime)
                {
                    ReplaceHint(internalAssemblyName, hintList);
                }
            }
            catch (Exception ex)
            {
                //Make sure to clear hint if error occurs
                _playerDisplay.InternalClearHint(internalAssemblyName);
                Logger.Instance.Error($"Error while generating hint for {internalAssemblyName}: {ex}");
            }
        }

        private void ReplaceHint(string assemblyName, IReadOnlyList<Hint> hints)
        {
            _playerDisplay.InternalClearHint(assemblyName);
            foreach(Hint hint in hints) _playerDisplay.InternalAddHint(assemblyName, hint);
            _playerDisplay.ForceUpdate();//Since all the CompatibilityAdaptor hint is not synced, we need to force update
        }

        private IReadOnlyList<Hint> ParseRichTextToHints(string content)
        {
            RichTextParser parser = _richTextParserPool.Rent();
            IReadOnlyList<LineInfo> lineInfoList = parser.ParseText(content, 40);
            _richTextParserPool.Return(parser);

            if (lineInfoList.IsEmpty())
            {
                return new List<Hint>();
            }

            float totalHeight = lineInfoList.Sum(x => x.Height);
            float accumulatedHeight = 0f;
            List<Hint> result = new(lineInfoList.Count);

            foreach (LineInfo lineInfo in lineInfoList)
            {
                //If not empty line, then add hint
                if (!string.IsNullOrEmpty(lineInfo.RawText.Trim()) && !lineInfo.Characters.IsEmpty())
                {
                    result.Add(new Hint
                    {
                        Text = lineInfo.RawText,
                        XCoordinate = lineInfo.Pos,
                        YCoordinate = 700 - totalHeight / 2 + lineInfo.Height + accumulatedHeight,
                        YCoordinateAlign = HintVerticalAlign.Bottom,
                        Alignment = lineInfo.Alignment,
                        FontSize = (int)lineInfo.Characters.First().FontSize,
                        SyncSpeed = HintSyncSpeed.UnSync // To make sure that when the compatibility adaptor is clearing the previous hint, the player display will not be updated
                    });
                }

                accumulatedHeight += lineInfo.Height;
            }

            return result.AsReadOnly();
        }
    }
}