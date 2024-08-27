using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using MEC;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PluginAPI.Helpers;
using YamlDotNet.Core.Tokens;

namespace HintServiceMeow.Core.Utilities
{
    /// <summary>
    /// Compatibility adapter for the other plugins
    /// </summary>
    internal static class CompatibilityAdapter
    {
        public static Dictionary<string, List<Hint>> AssemblyHint = new Dictionary<string, List<Hint>>();

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
            var positions = new List<TextPosition>();

            HeightResult heightResult = new HeightResult
            {
                HighestHeight = 0,
                LastingFontSize = 0
            };

            AlignmentResult alignmentResult = new AlignmentResult
            {
                Alignment = HintAlignment.Center,
                LastingAlignment = HintAlignment.Center
            };

            foreach (var text in textList)
            {
                var lastingHeight = heightResult.LastingFontSize;
                heightResult = GetHeight(text, lastingHeight);
                alignmentResult = GetAlignment(text, alignmentResult.LastingAlignment);

                positions.Add(new TextPosition
                {
                    Text = text,
                    Height = heightResult.HighestHeight,
                    Alignment = alignmentResult.Alignment,
                    FontSize = lastingHeight == 0 ? 40 : lastingHeight//Apply lasting height from last line
                });
            }

            var totalHeight = positions.Count > 0 ? positions.Sum(x => x.Height) : 0;
            var accumulatedHeight = 0f;
            
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

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{DateTime.Now.Ticks.ToString()}.text");
            File.WriteAllText(path, playerDisplay._lastText);
        }

        //Return a value tuple, value 1 is height, value 2 is last font size
        private static HeightResult GetHeight(string text, float LastFontSize)
        {
            text = text.Replace(" ", "");

            HeightResult result = new HeightResult
            {
                HighestHeight = LastFontSize,
                LastingFontSize = LastFontSize,
            };

            //Get the max font size
            MatchCollection sizeMatches = Regex.Matches(text, @"<size=(\d+)(px|%)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            //If no size tag is found, set the default value to 40
            if (sizeMatches.Count == 0)
            {
                result.HighestHeight = Math.Max(result.HighestHeight, 40);
            }
            else
            {
                foreach (Match match in sizeMatches)
                {
                    if (!match.Success)
                        continue;
                    
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

                    result.HighestHeight = Math.Max(result.HighestHeight, actualValue);
                }
            }

            //Check and handle lasting font size
            Stack<string> stack = new Stack<string>();

            MatchCollection matches = Regex.Matches(text, @"<size=(\d+)(px|%)?>|</size>", RegexOptions.IgnoreCase|RegexOptions.Compiled);

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
                        result.LastingFontSize = 0;
                    }
                }
            }

            //Find lasting font size
            if(stack.Count > 0)
            {
                string lastingSize = stack.Pop();
                var lastingSizeMatch = Regex.Match(lastingSize, @"<size=(\d+)(px|%)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (lastingSizeMatch.Success)
                {
                    int value = int.Parse(lastingSizeMatch.Groups[1].Value);
                    var unit = lastingSizeMatch.Groups[2].Value;
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

                    result.LastingFontSize = actualValue;
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
                        result.HighestHeight = value;
                        break;
                    case "%":
                        result.HighestHeight = result.HighestHeight * value / 100f;
                        break;
                    default:
                        result.HighestHeight = value;
                        break;
                }
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

            var matches = Regex.Matches(text, @"<align=(left|center|right)>|</align>", RegexOptions.IgnoreCase|RegexOptions.Compiled);

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

        private class HeightResult
        {
            public float HighestHeight { get; set; }

            public float LastingFontSize { get; set; }
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