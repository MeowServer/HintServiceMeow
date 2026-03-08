using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;

namespace HintServiceMeow.Benchmarks.Benchmarks
{
    // Struct to bind the specific parameter pairs together.
    public struct HintParams
    {
        public int RegularHints { get; set; }
        public int DynamicHints { get; set; }

        // Overriding ToString determines how it appears in the BenchmarkDotNet results table.
        public override string ToString() => $"{RegularHints}R_{DynamicHints}D";
    }

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class HintParserBenchmark
    {
        // Define the specific pairs of parameters to test.
        public IEnumerable<HintParams> GetParameters()
        {
            yield return new HintParams { RegularHints = 50, DynamicHints = 30 };
            yield return new HintParams { RegularHints = 30, DynamicHints = 10 };
        }

        // Use ParamsSource to inject the parameter pairs.
        [ParamsSource(nameof(GetParameters))]
        public HintParams CurrentParams { get; set; }

        public List<Hint> hints;
        public List<DynamicHint> dynamicHints;
        public HintCollection hintCollection;
        public HintCollection hintOnlyCollection;
        public HintCollection dynamicHintOnlyCollection;
        public List<string> testRichTexts;

        // Use GlobalSetup instead of the constructor. BenchmarkDotNet calls this 
        // AFTER injecting the parameters but BEFORE the benchmark starts.
        [GlobalSetup]
        public void Setup()
        {
            // Re-initialize collections for each parameter run to prevent data carryover.
            hints = new List<Hint>();
            dynamicHints = new List<DynamicHint>();
            hintCollection = new HintCollection();
            hintOnlyCollection = new HintCollection();
            dynamicHintOnlyCollection = new HintCollection();

            // Static constructor to initialize any static data if needed in the future.
            for (int i = 0; i < CurrentParams.RegularHints; i++)
            {
                var staticHint = new Hint
                {
                    // Intentionally mixing valid tags with illegal tags that need to be removed by Regex 
                    // to increase the pressure on Regex processing.
                    Text = $"<color=red>Static Block {i}</color> <pos={i}>Illegal</pos> <voffset=99>Tag</voffset>",
                    XCoordinate = (i % 30) * 40 - 600, // Evenly distributed across the X-axis from -600 to 600
                    YCoordinate = (i / 30) * 45,       // Evenly distributed across the Y-axis from 0 to 900
                    FontSize = 20 + (i % 10),
                    LineHeight = 1.2f,
                    Alignment = (HintAlignment)(i % 3)
                };
                hints.Add(staticHint);
                hintOnlyCollection.AddHint($"Assembly_Static_{i % 5}", staticHint);
                // Distribute into different Assemblies to increase the grouping overhead when traversing allGroups.
                hintCollection.AddHint($"Assembly_Static_{i % 5}", staticHint);
            }

            for (int i = 0; i < CurrentParams.DynamicHints; i++)
            {
                var dynamicHint = new DynamicHint
                {
                    Text = $"<size=24>Dynamic Competitor {i}</size> {{IllegalBraces}} <line-height=0>",
                    TargetX = 0,   // All attempting to crowd the center
                    TargetY = 500, // All attempting to crowd the center
                    LeftBoundary = -1000,
                    RightBoundary = 1000,
                    TopBoundary = 1000,
                    BottomBoundary = 0,
                    TopMargin = 2,
                    BottomMargin = 2,
                    LeftMargin = 5,
                    RightMargin = 5,
                    Priority = (HintPriority)(i % 5), // Mix different priorities to trigger the descending Sort logic for dynamic hints
                    Strategy = DynamicHintStrategy.StayInPosition // Force generation even if no position is found, increasing string concatenation load
                };
                dynamicHints.Add(dynamicHint);
                dynamicHintOnlyCollection.AddHint($"Assembly_Dynamic_{i % 3}", dynamicHint);
                hintCollection.AddHint($"Assembly_Dynamic_{i % 3}", dynamicHint);
            }

            // Use CurrentParams instead of the old RegularHints/DynamicHints fields.
            testRichTexts = new List<string>();
            for (int i = 0; i < CurrentParams.RegularHints; i++)
            {
                testRichTexts.Add($"<color=red>Static Block {i}</color> <pos={i}>Illegal</pos> <voffset=99>Tag</voffset>");
            }

            for (int i = 0; i < CurrentParams.DynamicHints; i++)
            {
                testRichTexts.Add($"<size=24>Dynamic Competitor {i}</size> {{IllegalBraces}} <line-height=0>");
            }
        }

        [Benchmark()]
        public void ParseHintAndDynamicHints()
        {
            // Fix: Removed the local empty 'HintCollection collection = new HintCollection();' 
            // and use the populated 'hintCollection' instead to ensure actual load testing.
            HintParser parser = new HintParser();

            HintParserResult result = parser.ParseToMessage(hintCollection);

            // Ensure the result is not optimized away by the compiler 
            // (If the framework requires assertions, a rough check on resultMessage.Length can be done here).
            if (string.IsNullOrEmpty(result.Content))
            {
                throw new Exception("Parsed message should not be empty in this complex scenario.");
            }
        }

        [Benchmark()]
        public void ParseHintsOnly()
        {
            HintParser parser = new HintParser();

            HintParserResult result = parser.ParseToMessage(hintOnlyCollection);

            if (string.IsNullOrEmpty(result.Content))
            {
                throw new Exception("Parsed message should not be empty in this complex scenario.");
            }
        }

        [Benchmark()]
        public void ParseDynamicHintsOnly()
        {
            HintParser parser = new HintParser();

            HintParserResult result = parser.ParseToMessage(dynamicHintOnlyCollection);

            if (string.IsNullOrEmpty(result.Content))
            {
                throw new Exception("Parsed message should not be empty in this complex scenario.");
            }
        }

        [Benchmark()]
        public void ParseRichText()
        {
            RichTextParser parser = new RichTextParser();

            foreach (string str in testRichTexts)
            {
                parser.ParseText(str, 20, HintAlignment.Center);
            }
        }
    }
}