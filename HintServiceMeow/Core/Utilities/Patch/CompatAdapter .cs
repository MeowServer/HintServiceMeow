using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities.Tools.Patch
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

        private static readonly string SizeTagRegex = @"<size=(\d+)(px|%)?>";

        private static readonly string LineHeightTagRegex = @"<line-height=([\d\.]+)(px|%|em)?>";

        private static readonly string AlignTagRegex = @"<align=(left|center|right)>|</align>";

        private static readonly string PosTagRegex = @"<pos=([+-]?\d+(px)?)>";

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

            _ = InternalShowHint(player, assemblyName, content, timeToRemove, cancellationTokenSource.Token);
        }

        public static async Task InternalShowHint(ReferenceHub player, string assemblyName, string content, float timeToRemove, CancellationToken cancellationToken)
        {
            var playerDisplay = PlayerDisplay.Get(player);
            assemblyName = "CompatibilityAdaptor-" + assemblyName;

            if (playerDisplay == null)
                return;

            //Reset the time to remove
            RemoveTime[assemblyName] = DateTime.Now.AddSeconds(timeToRemove);

            //Check if the hint is already cached

            HintCache.TryGetValue(content, out var cachedHintList);
            
            if(cachedHintList != null)
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
            var hintList = await Task.Run(() =>
            {
                var textList = content.Split('\n');
                var positions = new List<TextPosition>();

                HeightResult heightResult = new HeightResult
                {
                    Height = 0,
                    LastingFontTags = new Stack<string>()
                };

                AlignmentResult alignmentResult = new AlignmentResult
                {
                    Alignment = HintAlignment.Center,
                    LastingAlignment = HintAlignment.Center
                };

                foreach (var text in textList)
                {
                    int lastingHeight = heightResult.LastingFontTags.Count == 0 ? 40 : (int)ParseSizeTag(Regex.Match(heightResult.LastingFontTags.Peek(), SizeTagRegex));
                    heightResult = GetHeight(text, heightResult.LastingFontTags);
                    alignmentResult = GetAlignment(text, alignmentResult.LastingAlignment);

                    positions.Add(new TextPosition
                    {
                        Text = text,
                        Alignment = alignmentResult.Alignment,
                        Pos = GetPos(text),
                        Height = heightResult.Height,
                        FontSize = lastingHeight,
                    });
                }

                var totalHeight = positions.Count > 0 ? positions.Sum(x => x.Height) : 0;
                var accumulatedHeight = 0f;

                List<Hint> generatedHintList = new List<Hint>();

                foreach (var textPosition in positions)
                {
                    var hint = new CompatAdapterHint
                    {
                        Text = textPosition.Text,
                        XCoordinate = textPosition.Pos,
                        YCoordinate = 700 - totalHeight / 2 + textPosition.Height + accumulatedHeight,
                        YCoordinateAlign = HintVerticalAlign.Bottom,
                        Alignment = textPosition.Alignment,
                        FontSize = (int)textPosition.FontSize,
                    };

                    accumulatedHeight += textPosition.Height;
                    generatedHintList.Add(hint);
                }

                return generatedHintList;
            }, cancellationToken);

            if(cancellationToken.IsCancellationRequested)
                return;

            //Reset hint
            playerDisplay.InternalClearHint(assemblyName);
            playerDisplay.InternalAddHint(assemblyName, hintList);

            //Remove hint after time to remove
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeToRemove + 0.2f), cancellationToken);

                if(RemoveTime[assemblyName] <= DateTime.Now)
                    playerDisplay.InternalClearHint(assemblyName);
            }
            catch (TaskCanceledException) { }


            //Add generated hint into cache
            HintCache[content] = new List<Hint>(hintList);

            //Set cache expiration
            _ = Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ =>
            {
                HintCache.TryRemove(content, out var _);
            });
        }

        private static HeightResult GetHeight(string text, Stack<string> stack)
        {
            text = text.Replace(" ", "");

            float currentFontSize = 0;

            if(stack.Count != 0)
            {
               var match = Regex.Match(stack.Peek(), SizeTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
               currentFontSize = ParseSizeTag(match);
            }

            HeightResult result = new HeightResult
            {
                Height = currentFontSize,
                LastingFontTags = stack,
            };

            //Get the max font size
            MatchCollection sizeMatches = Regex.Matches(text, SizeTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            //If no size tag is found, set the default value to 40
            if (sizeMatches.Count == 0)
            {
                currentFontSize = Math.Max(result.Height, 40);
            }
            else
            {
                foreach (Match match in sizeMatches)
                {
                    if (!match.Success)
                        continue;

                    currentFontSize = Math.Max(result.Height, ParseSizeTag(match));
                }
            }

            //Check and handle lasting font size

            MatchCollection matches = Regex.Matches(text, @"<size=(\d+)(px|%)?>|</size>", RegexOptions.IgnoreCase|RegexOptions.Compiled);

            foreach (Match match in matches)
            {
                if (match.Value == "</size>")
                {
                    if (stack.Count > 0)
                    {
                        result.LastingFontTags.Pop();
                    }
                }
                else
                {
                    result.LastingFontTags.Push(match.Value);

                }
            }

            Match lineHeightMatch = Regex.Match(text, LineHeightTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var lineHeight = ParseLineHeightTag(lineHeightMatch, currentFontSize);
            result.Height = lineHeight >= 0? lineHeight : currentFontSize;

            return result;
        }

        private static AlignmentResult GetAlignment(string text, HintAlignment lastAlignment)
        {
            AlignmentResult result = new AlignmentResult
            {
                Alignment = lastAlignment,
                LastingAlignment = lastAlignment
            };

            var matches = Regex.Matches(text, AlignTagRegex, RegexOptions.IgnoreCase|RegexOptions.Compiled);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    if (match.Value == "</align>")
                    {
                        result.LastingAlignment = HintAlignment.Center;
                    }
                    else if (System.Enum.TryParse(match.Groups[1].Value, true, out HintAlignment alignment))
                    {
                        result.Alignment = alignment;
                        result.LastingAlignment = alignment;
                    }
                }
            }

            return result;
        }

        private static float GetPos(string text)
        {
            var match = Regex.Match(text, PosTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (match.Success)
            {
                return float.Parse(match.Groups[1].Value);
            }

            return 0;
        }

        private static float ParseSizeTag(Match match)
        {
            var value = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value;

            switch (unit.ToLower())
            {
                case "px":
                    return value;
                case "%":
                    return 40f * value / 100f;
                default:
                    return value;
            }
        }

        private static float ParseLineHeightTag(Match lineHeightMatch, float fontSize)
        {
            if (lineHeightMatch.Success)
            {
                var value = float.Parse(lineHeightMatch.Groups[1].Value);
                var unit = lineHeightMatch.Groups[2].Value;

                switch (unit.ToLower())
                {
                    case "px":
                        return value;
                    case "%":
                        return fontSize * value / 100f;
                    case "em":
                            return fontSize * value;
                    default:
                        return value;
                }
            }

            return -1;
        }

        private class HeightResult
        {
            public float Height { get; set; }

            public Stack<string> LastingFontTags { get; set; }
        }

        private class AlignmentResult
        {
            public HintAlignment Alignment { get; set; }

            public HintAlignment LastingAlignment { get; set; }
        }

        private class TextPosition
        {
            public string Text { get; set; }

            public HintAlignment Alignment { get; set; }

            public float Pos { get; set; }

            public float Height { get; set; }

            public float FontSize { get; set; }
        }
    }
}