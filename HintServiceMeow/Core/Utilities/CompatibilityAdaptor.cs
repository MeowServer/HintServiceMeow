using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Core.Utilities.Tools;
using MEC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Used to adapt other plugins' hint system to HintServiceMeow's hint system
    /// </summary>
    internal class CompatibilityAdaptor : ICompatibilityAdaptor
    {
        private static readonly Cache<string, IReadOnlyList<Hint>> HintCache = new(500);

        private readonly ConcurrentDictionary<string, int> _removeTickets = new();

        private readonly PlayerDisplay _playerDisplay;

        internal CompatibilityAdaptor(PlayerDisplay playerDisplay)
        {
            this._playerDisplay = playerDisplay ?? throw new ArgumentNullException(nameof(playerDisplay));
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
                return;
            }

            //Create new remove ticket to stop last remove action
            int removeTicket = _removeTickets.AddOrUpdate(internalAssemblyName, 0, (_, oldValue) => oldValue + 1);

            //Start the new remove action
            Timing.CallDelayed(duration + 0.1f, () => //Add an extra 0.1 second to prevent blinking
            {
                //Check if the current remove ticket is same as the one passed in
                if (_removeTickets.TryGetValue(internalAssemblyName, out int currentRemoveTicket) && currentRemoveTicket == removeTicket)
                    _playerDisplay.InternalClearHint(internalAssemblyName);
            });

            DateTime expireTime = DateTime.Now.AddSeconds(Math.Min(duration, 1f));// Wait for at most 1 second and at least the duration

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
                List<Hint> hintList = await Task.Run(() => this.ParseRichTextToHints(content));

                //Cache
                HintCache.Add(content, new List<Hint>(hintList).AsReadOnly());

                // Return if the content is already outdated
                if (DateTime.Now > expireTime)
                    return;

                ReplaceHint(internalAssemblyName, hintList);
            }
            catch (Exception ex)
            {
                //Make sure to clear hint if error occurs
                _playerDisplay.InternalClearHint(internalAssemblyName);
                LogTool.Error($"Error while generating hint for {internalAssemblyName}: {ex}");
            }
        }

        private void ReplaceHint(string name, IReadOnlyList<Hint> hints)
        {
            _playerDisplay.InternalClearHint(name);
            _playerDisplay.InternalAddHint(name, hints);
            _playerDisplay.ForceUpdate();//Since all the CompatibilityAdaptor hint is not synced, we need to force update
        }

        private List<Hint> ParseRichTextToHints(string content)
        {
            IReadOnlyList<LineInfo> lineInfoList = RichTextParserPool.ParseText(content, 40);

            if (lineInfoList.IsEmpty())
                return new List<Hint>();

            float totalHeight = lineInfoList.Sum(x => x.Height);
            float accumulatedHeight = 0f;
            List<Hint> result = new();

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
                        SyncSpeed = HintSyncSpeed.UnSync //To make sure that when the compatibility adaptor is clearing the previous hint, the player display will not be updated
                    });
                }

                accumulatedHeight += lineInfo.Height;
            }

            return result;
        }
    }
}