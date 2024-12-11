using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
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
        private static readonly ConcurrentDictionary<string, IReadOnlyList<Hint>> HintCache = new ConcurrentDictionary<string, IReadOnlyList<Hint>>();

        private readonly ConcurrentDictionary<string, DateTime> _removeTime = new ConcurrentDictionary<string, DateTime>();
        private readonly HashSet<string> _suppressedAssemblies = new HashSet<string>();

        private readonly TimeSpan _suppressionDuration = TimeSpan.FromSeconds(0.45f);

        private readonly PlayerDisplay _playerDisplay;

        internal CompatibilityAdaptor(PlayerDisplay playerDisplay)
        {
            this._playerDisplay = playerDisplay;
        }

        public void ShowHint(CompatibilityAdaptorArg ev)
        {
            string assemblyName = ev.AssemblyName;
            string content = ev.Content;
            float duration = ev.Duration;

            GetCompatAssemblyName.RegisteredAssemblies.Add(assemblyName);

            if (PluginConfig.Instance.DisabledCompatAdapter.Contains(assemblyName) //Config limitation
                || content.Length > ushort.MaxValue //Length limitation
                || _suppressedAssemblies.Contains(assemblyName)) //Rate limitation
                return;

            //Rate limitation
            _suppressedAssemblies.Add(assemblyName);
            Timing.CallDelayed((float)_suppressionDuration.TotalSeconds, () => _suppressedAssemblies.Remove(assemblyName));

            string internalAssemblyName = "CompatibilityAdaptor-" + assemblyName;

            //For negative duration, clear hint
            if (duration <= 0f)
            {
                _playerDisplay.InternalClearHint(internalAssemblyName);
                return;
            }

            duration = Math.Min(duration, float.MaxValue - 1f);

            _removeTime[internalAssemblyName] = DateTime.Now.AddSeconds(duration);

            //Stop previous remove action and start the new one
            Timing.CallDelayed(duration + 0.1f, () => //Add an extra 0.1 second to prevent blinking
            {
                if (_removeTime.TryGetValue(internalAssemblyName, out DateTime removeTime) && removeTime < DateTime.Now)
                    _playerDisplay.InternalClearHint(internalAssemblyName);
            });

            //Start new remove action, remove after the Duration
            _ = InternalShowHint(internalAssemblyName, content, DateTime.Now.AddSeconds(duration));
        }

        private async Task InternalShowHint(string internalAssemblyName, string content, DateTime expireTime)
        {
            try
            {
                //Check if the hint is already cached
                if (HintCache.TryGetValue(content, out IReadOnlyList<Hint> cachedHintList))
                {
                    _playerDisplay.InternalClearHint(internalAssemblyName, false);
                    _playerDisplay.InternalAddHint(internalAssemblyName, cachedHintList);

                    return;
                }

                DateTime startTime = DateTime.Now;

                //Parse the content to hints
                List<Hint> hintList = await Task.Run(() => this.ParseRichTextToHints(content, internalAssemblyName));

                //Cache
                this.AddToCache(content, new List<Hint>(hintList).AsReadOnly());

                //Make sure for low performance server or if the Duration is shorter than converting time.
                if (DateTime.Now - startTime > _suppressionDuration || DateTime.Now > expireTime)
                    return;

                _playerDisplay.InternalClearHint(internalAssemblyName, false);
                _playerDisplay.InternalAddHint(internalAssemblyName, hintList);
            }
            catch (Exception ex)
            {
                //Make sure to clear hint if error occurs
                _playerDisplay.InternalClearHint(internalAssemblyName);
                LogTool.Error($"Error while generating hint for {internalAssemblyName}: {ex}");
            }
        }

        private List<Hint> ParseRichTextToHints(string content, string internalAssemblyName)
        {
            IReadOnlyList<LineInfo> lineInfoList = RichTextParserPool.ParseText(content, 40);

            if (lineInfoList is null || lineInfoList.IsEmpty())
                return new List<Hint>();

            float totalHeight = lineInfoList.Sum(x => x.Height);
            float accumulatedHeight = 0f;
            List<Hint> generatedHintList = new List<Hint>();

            foreach (LineInfo lineInfo in lineInfoList)
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

        private void AddToCache(string content, IReadOnlyList<Hint> hintList)
        {
            if (!HintCache.TryAdd(content, hintList))
                return;

            //Remove after certain amount of time
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(15000);
                    HintCache.TryRemove(content, out _);
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
            });
        }
    }
}