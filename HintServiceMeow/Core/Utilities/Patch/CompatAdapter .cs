using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using MEC;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HintServiceMeow.Core.Utilities.Patch
{
    /// <summary>
    /// Compatibility adapter for the other plugins
    /// </summary>
    internal static class CompatibilityAdapter
    {
        private static readonly Dictionary<string, DateTime> RemoveTime = new Dictionary<string, DateTime>();

        private static readonly string SizeTagRegex = @"<size=(\d+)(px|%)?>";

        private static readonly string LineHeightTagRegex = @"<line-height=([\d\.]+)(px|%)?>";

        private static readonly string AlignTagRegex = @"<align=(left|center|right)>|</align>";

        private static readonly Dictionary<string, List<Hint>> HintCache = new Dictionary<string, List<Hint>>();

        public static void ShowHint(ReferenceHub player, string assemblyName, string content, float timeToRemove)
        {
            assemblyName = "CompatibilityAdapter-" + assemblyName;

            var playerDisplay = PlayerDisplay.Get(player);

            if (playerDisplay == null)
                return;

            //Clear previous hints
            playerDisplay.InternalClearHint(assemblyName);

            //Set the time to remove
            RemoveTime[assemblyName] = DateTime.Now.AddSeconds(timeToRemove);

            //Check if the hint is already cached
            if (HintCache.TryGetValue(content, out var cachedHintList))
            {
                playerDisplay.InternalAddHint(assemblyName, cachedHintList);

                Timing.CallDelayed(timeToRemove + 0.1f, () =>
                {
                    if(playerDisplay != null && RemoveTime[assemblyName] <= DateTime.Now)
                        playerDisplay.InternalClearHint(assemblyName);
                });

                return;
            }

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
                    Height = heightResult.Height,
                    Alignment = alignmentResult.Alignment,
                    FontSize = lastingHeight //Apply lasting height from last line
                });
            }

            var totalHeight = positions.Count > 0 ? positions.Sum(x => x.Height) : 0;
            var accumulatedHeight = 0f;
            
            List<Hint> hintList = new List<Hint>();

            foreach(var textPosition in positions)
            {
                var hint = new CompatAdapterHint
                {
                    Text = textPosition.Text,
                    YCoordinate = 700 - totalHeight / 2 + textPosition.Height + accumulatedHeight,
                    YCoordinateAlign = HintVerticalAlign.Bottom,
                    Alignment = textPosition.Alignment,
                    FontSize = (int)textPosition.FontSize,
                };

                accumulatedHeight += textPosition.Height;
                hintList.Add(hint);
            }

            playerDisplay.InternalAddHint(assemblyName, hintList);

            HintCache.Add(content, new List<Hint>(hintList));

            Timing.CallDelayed(timeToRemove + 0.1f, () =>
            {
                if(playerDisplay != null && RemoveTime[assemblyName] <= DateTime.Now)
                     playerDisplay.InternalClearHint(assemblyName);
            });
        }

        //Return a value tuple, value 1 is height, value 2 is last font size
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
            if (lineHeight >= 0)
            {
                result.Height = lineHeight;
            }
            else
            {
                result.Height = currentFontSize;
            }

            return result;
        }

        //Return a value tuple, value 1 is alignment, value 2 is lasting alignment
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

        private static float ParseSizeTag(Match match)
        {
            var value = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value;

            switch (unit)
            {
                case "px":
                    return value;
                case "%":
                    return 40f * value / 100f;
                default:
                    return value;
            }

            return -1;
        }

        private static float ParseLineHeightTag(Match lineHeightMatch, float fontSize)
        {
            if (lineHeightMatch.Success)
            {
                var value = float.Parse(lineHeightMatch.Groups[1].Value);
                var unity = lineHeightMatch.Groups[2].Value;

                switch (unity)
                {
                    case "px":
                        return value;
                    case "%":
                        return fontSize * value / 100f;
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

            public float Height { get; set; }

            public float FontSize { get; set; }
        }
    }
}