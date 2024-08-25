using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using MEC;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Tokens;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Compatibility adapter for the other plugins
    /// </summary>
    internal static class CompatibilityAdapter
    {
        public static Dictionary<string, List<Hint>> AssemblyHint = new Dictionary<string, List<Hint>>();

        private static readonly Regex SizeRegex = new Regex(@"<size=(\d+)>");
        private static readonly Regex LineHeightRegex = new Regex(@"<line-height=([0-9\.]+%?)>");

        public static void ShowHint(ReferenceHub player, string assembly, string content, float timeToRemove)
        {
            var playerDisplay = PlayerDisplay.Get(player);

            if (AssemblyHint.TryGetValue(assembly, out var hints))
            {
                playerDisplay.RemoveHint(hints);
                hints.Clear();
            }
            else
            {
                AssemblyHint.Add(assembly, new List<Hint>());
            }

            var textList = content.Split('\n');
            var heightList = new List<ValueTuple<string, float>>();

            float lastFontSize = 0f;

            foreach (var text in textList)
            {
                var value = GetHeight(text, lastFontSize);
                lastFontSize = value.Item2;
                heightList.Add(ValueTuple.Create(text, value.Item1));
            }

            var totalHeight = heightList.Count > 0 ? heightList.Sum(x => x.Item2) : 0;
            var accumulatedHeight = 0f;

            HintAlignment lastAlignment = HintAlignment.Center;

            foreach(var kvp in heightList)
            {
                var alignResult = GetAlignment(kvp.Item1, lastAlignment);
                lastAlignment = alignResult.Item2;

                var hint = new Hint
                {
                    Text = kvp.Item1,
                    YCoordinate = 700 - totalHeight / 2 + kvp.Item2 + accumulatedHeight,
                    YCoordinateAlign = HintVerticalAlign.Bottom,
                    Alignment = alignResult.Item1,
                };

                accumulatedHeight += kvp.Item2;

                playerDisplay.AddHint(hint);
                AssemblyHint[assembly].Add(hint);
            }

            var hintList = new List<Hint>(AssemblyHint[assembly]);
            Timing.CallDelayed(timeToRemove, () =>
            {
                foreach(var hint in hintList)
                    if(hint != null && playerDisplay != null)
                        playerDisplay.RemoveHint(hint);
            });
        }

        //Return a value tuple, value 1 is height, value 2 is last font size
        private static ValueTuple<float, float> GetHeight(string text, float LastFontSize)
        {
            ValueTuple<float, int> result = ValueTuple.Create(0f, 0);

            float maxFontSize = LastFontSize;

            //Get the max font size
            MatchCollection sizeMatches = Regex.Matches(text, @"<size=(\d+)(px|%)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (Match match in sizeMatches)
            {
                if (match.Success)
                {
                    var value = int.Parse(match.Groups[1].Value);
                    var unit = match.Groups[2].Value;
                    var actualValue = 0f;

                    switch (unit)
                    {
                        case "px":
                            actualValue = value;
                            break;
                        case "%":
                            actualValue = 40f * value / 100f;
                            break;
                        default:
                            actualValue = value;
                            break;
                    }

                    maxFontSize = Math.Max(maxFontSize, actualValue);
                }
            }
            //If no size tag is found, set the default value to 40
            if (sizeMatches.Count == 0)
            {
                maxFontSize = 40;
            }

            //Check and handle lasting font size
            Stack<string> stack = new Stack<string>();

            MatchCollection matches = Regex.Matches(text, @"<size=(\d+)(px|%)?>|<\\size>", RegexOptions.IgnoreCase|RegexOptions.Compiled);

            foreach (Match match in matches)
            {
                if (match.Value.StartsWith("<size="))
                {
                    stack.Push(match.Value);
                }
                else
                {
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        LastFontSize = 0;
                    }
                }
            }

            //Find lasting font size
            if(stack.Count > 0)
            {
                string lastSize = stack.Pop();
                var lastSizeMatch = Regex.Match(lastSize, @"<size=(\d+)(\w*)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (lastSizeMatch.Success)
                {
                    int value = int.Parse(lastSizeMatch.Groups[1].Value);
                    var unit = lastSizeMatch.Groups[2].Value;
                    var actualValue = 0f;

                    switch (unit)
                    {
                        case "px":
                            actualValue = value;
                            break;
                        case "%":
                            actualValue = 40f * value / 100f;
                            break;
                        default:
                            actualValue = value;
                            break;
                    }

                    result.Item2 = value;
                }
            }

            //Get line height and calculate actual height
            Match lineHeightMatches = Regex.Match(text, @"<line-height=([\d\.]+)(px|%)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            if (lineHeightMatches.Success)
            {
                var value = float.Parse(lineHeightMatches.Groups[1].Value);
                var unity = lineHeightMatches.Groups[2].Value;

                switch (unity)
                {
                    case "px":
                        result.Item1 = value;
                        break;
                    case "%":
                        result.Item1 = maxFontSize * value / 100f;
                        break;
                    default:
                        result.Item1 = value;
                        break;
                }
            }
            else//If no line height tag is found, set the result to 0
            {
                result.Item1 = maxFontSize;
            }

            return result;
        }

        //Return a value tuple, value 1 is alignment, value 2 is lasting alignment
        private static ValueTuple<HintAlignment, HintAlignment> GetAlignment(string text, HintAlignment lastAlignment)
        {
            ValueTuple<HintAlignment, HintAlignment> result = ValueTuple.Create(lastAlignment, lastAlignment);

            var matches = Regex.Matches(text, @"<align=(left|center|right)>|</align>", RegexOptions.IgnoreCase|RegexOptions.Compiled);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    if (match.Value == "</align>")
                    {
                        result.Item2 = HintAlignment.Center;
                    }
                    else if (System.Enum.TryParse(match.Groups[1].Value, true, out HintAlignment alignment))
                    {
                        result.Item1 = alignment;
                        result.Item2 = alignment;
                    }
                }
            }

            return result;
        }
    }
}