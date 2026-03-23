namespace HintServiceMeow.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Models.Parser;
    using HintServiceMeow.Core.Models.Parser.Style;
    using HintServiceMeow.Core.Utilities.Parser;
    using HintServiceMeow.Core.Utilities.Pools;
    using HintServiceMeow.Core.Utilities.Tools;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;
    using HintServiceMeow.Plugin;

    /// <summary>
    /// Adapt raw unity rich text into HSM.
    /// </summary>
    internal class CompatibilityAdaptor : ICompatibilityAdaptor
    {
        internal static readonly HashSet<string> RegisteredAssemblies = new(); // All assemblies that used compatibility adaptor
        private static readonly ICache<string, IReadOnlyList<Hint>> HintCache = new Cache<string, IReadOnlyList<Hint>>(500);

        private readonly Dictionary<string, ICoroutine> removeHandles = new();
        private readonly PlayerDisplay playerDisplay; // Initialize in constructor
        private readonly IPool<RichTextParser> richTextParserPool; // Initialize in constructor
        private readonly ICoroutineRunner coroutineRunner; // Initialize in constructor

        private readonly RichTextParserSetting settingTemplate = new RichTextParserSetting(
            TextMeshStyle.Default,
            [],
            [],
            ["a", "allcaps", "alpha", "b", "color", "font", "font-weight", "gradient", "i", "lowercase", "mark", "noparse", "s", "smallcaps", "style", "u", "uppercase", "link"],
            true); // Tags that does not affect the position of the hint are ignored

        private bool destructed; // To prevent multiple destruct calls

        internal CompatibilityAdaptor(
            PlayerDisplay playerDisplay,
            IPool<RichTextParser>? richTextParserPool = null,
            ICoroutineRunner? coroutineRunner = null)
        {
            this.playerDisplay = playerDisplay ?? throw new ArgumentNullException(nameof(playerDisplay));
            this.richTextParserPool = richTextParserPool ?? RichTextParserPool.Instance;
            this.coroutineRunner = coroutineRunner ?? new UnityCoroutineRunner();
        }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            if (destructed)
                return;

            foreach (ICoroutine coroutine in removeHandles.Values)
            {
                coroutine.Kill(); // Stop all running coroutines
            }

            removeHandles.Clear(); // Clear the dictionary

            destructed = true; // Mark as destructed
        }

        public void ShowHint(CompatibilityAdaptorArg ev)
        {
            if (ev is null)
                throw new ArgumentNullException(nameof(ev));

            string assemblyName = ev.AssemblyName;
            string content = ev.Content ?? string.Empty;
            float duration = Math.Min(ev.Duration, float.MaxValue - 1f);

            // Record the assembly that is using the compatibility adaptor
            RegisteredAssemblies.Add(assemblyName);

            if (Plugin.Instance.Config.DisabledCompatAssemblies.Any(x => assemblyName.Contains(x)) // Config limitation
                || content.Length > ushort.MaxValue) // Length limitation
                return;

            // Use internal assembly name to ensure safety
            string internalAssemblyName = "CompatibilityAdaptor-" + assemblyName;

            // For negative duration or empty content, clear hint
            if (duration <= 0f || string.IsNullOrEmpty(content))
            {
                playerDisplay.InternalClearHint(internalAssemblyName);
                playerDisplay.ForceUpdate();
                return;
            }

            // Clear hint after duration + 0.1 seconds
            if (destructed) // If the adaptor has been destructed, do not proceed
                return;

            if (removeHandles.TryGetValue(internalAssemblyName, out ICoroutine oldHandle))
                oldHandle.Kill(); // Stop the previous coroutine if exists

            // Start a new coroutine to remove the hint after the duration
            removeHandles[internalAssemblyName] =
                coroutineRunner.CallAfter(TimeSpan.FromSeconds(duration + 0.1f), () =>
                {
                    playerDisplay.InternalClearHint(internalAssemblyName);
                    playerDisplay.ForceUpdate();
                    removeHandles.Remove(internalAssemblyName);
                });

            DateTime expireTime = DateTime.Now.AddSeconds(Math.Min(duration, 5f)); // Wait for at most 5 second and at least the duration

            // Start new remove action, remove after the Duration
            _ = InternalShowHint(internalAssemblyName, content, expireTime);
        }

        private async Task InternalShowHint(string internalAssemblyName, string content, DateTime expireTime)
        {
            try
            {
                // Check if the hint is already cached
                if (HintCache.TryGet(content, out IReadOnlyList<Hint> cachedHintList))
                {
                    ReplaceHint(internalAssemblyName, cachedHintList);
                    return;
                }

                // Parse the content to hints
                IReadOnlyList<Hint> hintList = await ConcurrentTaskDispatcher.Instance.
                    Enqueue(() => Task.FromResult(ParseRichTextToHints(content)))
                    .ConfigureAwait(false);

                // Add result to cache
                HintCache.Add(content, hintList);

                // Update if the content is not outdated
                if (DateTime.Now < expireTime)
                {
                    ReplaceHint(internalAssemblyName, hintList);
                }
            }
            catch (Exception ex)
            {
                // Make sure to clear hint if error occurs
                playerDisplay.InternalClearHint(internalAssemblyName);
                Logger.Instance.Error($"Error while generating hint for {internalAssemblyName}: {ex}");
            }
        }

        private void ReplaceHint(string assemblyName, IReadOnlyList<Hint> hints)
        {
            playerDisplay.InternalClearHint(assemblyName);
            foreach (Hint hint in hints)
                playerDisplay.InternalAddHint(assemblyName, hint);
            playerDisplay.ForceUpdate();// Since all the CompatibilityAdaptor hint is not synced, we need to force update
        }

        private IReadOnlyList<Hint> ParseRichTextToHints(string content)
        {
            RichTextParser parser = richTextParserPool.Rent();
            LineInfo[] lineInfoList = parser.ParseText(content, settingTemplate).LineInfos;
            richTextParserPool.Return(parser);

            if (lineInfoList.IsEmpty())
            {
                return new List<Hint>();
            }

            float totalHeight = lineInfoList.Sum(x => x.Height);
            float accumulatedHeight = 0f;
            List<Hint> result = new(lineInfoList.Length);

            foreach (LineInfo lineInfo in lineInfoList)
            {
                // If not empty line, then add hint
                if (!string.IsNullOrEmpty(lineInfo.CleanText.Trim()) && !lineInfo.CharacterInfos.IsEmpty())
                {
                    result.Add(new Hint
                    {
                        Text = lineInfo.CleanText,
                        YCoordinate = 700 - (totalHeight / 2) + lineInfo.Height + accumulatedHeight,
                        YCoordinateAlign = HintVerticalAlign.Bottom,
                        Alignment = lineInfo.Style.Alignment,
                        FontSize = (int)lineInfo.CharacterInfos.First().Style.FontSize,
                        SyncSpeed = HintSyncSpeed.UnSync, // To make sure that when the compatibility adaptor is clearing the previous hint, the player display will not be updated
                    });
                }

                accumulatedHeight += lineInfo.Height;
            }

            return result.AsReadOnly();
        }
    }
}