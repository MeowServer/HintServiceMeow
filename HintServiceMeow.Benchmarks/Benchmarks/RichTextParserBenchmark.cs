using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Parser.Style;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Benchmarks.Benchmarks
{
    // ═══════════════════════════════════════════════════════════════════════════
    // 1. Full pipeline split by phase — locate time cost ratio of Tokenizer vs Parser
    // ═══════════════════════════════════════════════════════════════════════════

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class PhaseBenchmark
    {
        private string testInput;
        private RichTextParserSetting setting;
        private RichTextParser parser;

        // Sink fields — prevent JIT from optimizing away the entire call
        private int sink;

        [GlobalSetup]
        public void Setup()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                sb.Append($"<color=red><b>Block {i}</b></color> ")
                  .Append($"<size=24>sized text {i}</size> ")
                  .Append($"<alpha=#80>alpha</alpha> ")
                  .Append($"normal text line {i}")
                  .Append("\\n");
            }

            testInput = sb.ToString();

            setting = new RichTextParserSetting(
                TextMeshStyle.Default,
                Array.Empty<Tuple<string, IHintParameter>>(),
                Array.Empty<string>(),
                new HashSet<string>());

            parser = new RichTextParser();
        }

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Phase")]
        public void FullPipeline()
        {
            var result = parser.ParseText(testInput, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        [BenchmarkCategory("Phase")]
        public void TokenizeOnly()
        {
            var tokenizer = TokenizerPool.Instance.Rent();
            var tokens = tokenizer.Tokenize(testInput, setting.Parameters);
            sink = tokens.Count;
            TokenizerPool.Instance.Return(tokenizer);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 2. Split by input profile — identify which input pattern is most expensive
    // ═══════════════════════════════════════════════════════════════════════════

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class InputProfileBenchmark
    {
        private string plainText;
        private string tagHeavy;
        private string longSingleLine;
        private string manyShortLines;
        private string nestedStyles;
        private string colorHeavy;
        private string realWorldMixed;

        private RichTextParserSetting setting;
        private RichTextParser parser;
        private int sink;

        [GlobalSetup]
        public void Setup()
        {
            setting = new RichTextParserSetting(
                TextMeshStyle.Default,
                Array.Empty<Tuple<string, IHintParameter>>(),
                Array.Empty<string>(),
                new HashSet<string>());

            parser = new RichTextParser();

            var sb = new StringBuilder();
            for (int i = 0; i < 500; i++)
                sb.Append("Hello World ");
            plainText = sb.ToString();

            sb.Clear();
            for (int i = 0; i < 100; i++)
            {
                sb.Append($"<color=#{i:X2}{i:X2}{i:X2}><b><i><size={10 + i % 30}>X</size></i></b></color>");
            }
            tagHeavy = sb.ToString();

            sb.Clear();
            sb.Append("<color=red>");
            for (int i = 0; i < 2000; i++)
                sb.Append('A');
            sb.Append("</color>");
            longSingleLine = sb.ToString();

            sb.Clear();
            for (int i = 0; i < 200; i++)
                sb.Append($"Line{i}\\n");
            manyShortLines = sb.ToString();

            sb.Clear();
            for (int i = 0; i < 30; i++)
                sb.Append($"<color=#{i * 8:X2}0000><size={10 + i}><b>");
            sb.Append("DeepNested");
            for (int i = 0; i < 30; i++)
                sb.Append("</b></size></color>");
            nestedStyles = sb.ToString();

            sb.Clear();
            for (int i = 0; i < 100; i++)
            {
                if (i % 3 == 0)
                    sb.Append("<color=red>R</color>");
                else if (i % 3 == 1)
                    sb.Append($"<color=#{i:X2}{255 - i:X2}00>H</color>");
                else
                    sb.Append($"<color=#{i:X2}{i:X2}{i:X2}{i:X2}>A</color>");
            }
            colorHeavy = sb.ToString();

            sb.Clear();
            sb.Append("<align=center><size=30><b>Server Name</b></size></align>\\n");
            sb.Append("<color=#AAAAAA><size=18>Welcome to the server!</size></color>\\n");
            sb.Append("<align=left><indent=20><color=yellow>Player1</color> - 100 HP</indent></align>\\n");
            sb.Append("<align=left><indent=20><color=green>Player2</color> - <b>85</b> HP</indent></align>\\n");
            sb.Append("<line-height=2em>\\n");
            sb.Append("<margin=50><size=14><i>Round 3 of 10</i></size></margin>\\n");
            sb.Append("<alpha=#80><size=12>Press TAB for scoreboard</size></alpha>\\n");
            for (int i = 0; i < 5; i++)
            {
                sb.Append($"<color=#{(i * 50):X2}FF{(255 - i * 50):X2}><size={14 + i * 2}>Item {i}: <b>{100 + i * 10}</b> pts</size></color>\\n");
            }
            realWorldMixed = sb.ToString();
        }

        [Benchmark(Baseline = true)]
        public void PlainText()
        {
            var result = parser.ParseText(plainText, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void TagHeavy()
        {
            var result = parser.ParseText(tagHeavy, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void LongSingleLine()
        {
            var result = parser.ParseText(longSingleLine, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void ManyShortLines()
        {
            var result = parser.ParseText(manyShortLines, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void NestedStyles()
        {
            var result = parser.ParseText(nestedStyles, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void ColorHeavy()
        {
            var result = parser.ParseText(colorHeavy, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void RealWorldMixed()
        {
            var result = parser.ParseText(realWorldMixed, setting);
            sink = result.LineInfos.Length;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 3. Component-level microbenchmarks — isolate the cost of individual hot functions
    // ═══════════════════════════════════════════════════════════════════════════

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class ComponentBenchmark
    {
        [Benchmark]
        [BenchmarkCategory("TagChecker")]
        public bool TagChecker_IsValidTag()
        {
            bool result = false;
            for (int i = 0; i < 100; i++)
            {
                result |= TagChecker.IsValidTag("color");
                result |= TagChecker.IsValidTag("b");
                result |= TagChecker.IsValidTag("size");
                result |= TagChecker.IsValidTag("invalidtag");
                result |= TagChecker.IsSelfClosingTag("alpha");
                result |= TagChecker.IsSelfClosingTag("br");
            }

            return result;
        }

        [Benchmark]
        [BenchmarkCategory("FontTool")]
        public float FontTool_GetCharWidth()
        {
            float total = 0f;
            var fontTool = FontTool.Instance;
            string sample = "The quick brown fox jumps over the lazy dog. 1234567890!@#$%";
            for (int repeat = 0; repeat < 20; repeat++)
            {
                for (int i = 0; i < sample.Length; i++)
                {
                    total += fontTool.GetCharWidth(sample[i], 16f);
                }
            }

            return total;
        }

        [Benchmark]
        [BenchmarkCategory("Pool")]
        public void StringBuilderPool_RentReturn()
        {
            for (int i = 0; i < 100; i++)
            {
                var sb = StringBuilderPool.Instance.Rent();
                sb.Append("test data");
                StringBuilderPool.Instance.Return(sb);
            }
        }

        [Benchmark]
        [BenchmarkCategory("Pool")]
        public void TokenizerPool_RentReturn()
        {
            for (int i = 0; i < 100; i++)
            {
                var t = TokenizerPool.Instance.Rent();
                TokenizerPool.Instance.Return(t);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 4. Allocation hotspots — pinpoint the sources of GC pressure
    // ═══════════════════════════════════════════════════════════════════════════

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class AllocationBenchmark
    {
        private RichTextParserSetting setting;
        private RichTextParser parser;
        private int sink;

        [GlobalSetup]
        public void Setup()
        {
            setting = new RichTextParserSetting(
                TextMeshStyle.Default,
                Array.Empty<Tuple<string, IHintParameter>>(),
                Array.Empty<string>(),
                new HashSet<string>());
            parser = new RichTextParser();
        }

        [Benchmark(Baseline = true)]
        public void Alloc_PureText_500Chars()
        {
            var result = parser.ParseText(new string('A', 500), setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void Alloc_StyleSwitch_100Times()
        {
            var sb = new StringBuilder(2000);
            for (int i = 0; i < 100; i++)
                sb.Append("<b>A</b>BBBB");
            var result = parser.ParseText(sb.ToString(), setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void Alloc_100Lines()
        {
            var sb = new StringBuilder(600);
            for (int i = 0; i < 100; i++)
                sb.Append("X\\n");
            var result = parser.ParseText(sb.ToString(), setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void Alloc_Tokenizer_100Tags()
        {
            var sb = new StringBuilder(3000);
            for (int i = 0; i < 100; i++)
                sb.Append($"<color=#{i:X2}0000>X</color>");

            var tokenizer = TokenizerPool.Instance.Rent();
            var result = tokenizer.Tokenize(sb.ToString(), setting.Parameters);
            sink = result.Count;
            TokenizerPool.Instance.Return(tokenizer);
        }

        [Benchmark]
        public void Alloc_ToArray_50Lines()
        {
            var sb = new StringBuilder(2000);
            for (int i = 0; i < 50; i++)
                sb.Append("12345678901234567890\\n");
            var result = parser.ParseText(sb.ToString(), setting);
            sink = result.LineInfos.Length;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 5. Scalability tests — performance curve as input size grows
    // ═══════════════════════════════════════════════════════════════════════════

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class ScalabilityBenchmark
    {
        [Params(1, 5, 10, 50, 100)]
        public int Scale;

        private string scaledInput;
        private RichTextParserSetting setting;
        private RichTextParser parser;
        private int sink;

        [GlobalSetup]
        public void Setup()
        {
            setting = new RichTextParserSetting(
                TextMeshStyle.Default,
                Array.Empty<Tuple<string, IHintParameter>>(),
                Array.Empty<string>(),
                new HashSet<string>());

            parser = new RichTextParser();

            var sb = new StringBuilder();
            for (int i = 0; i < Scale; i++)
            {
                sb.Append($"<color=red><b>Block {i}</b></color> normal text <size=24>sized</size>\\n");
            }

            scaledInput = sb.ToString();
        }

        [Benchmark]
        public void Parse_Scaled()
        {
            var result = parser.ParseText(scaledInput, setting);
            sink = result.LineInfos.Length;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 6. IgnoreTags overhead test — your real-world config has a large number of IgnoreTags
    // ═══════════════════════════════════════════════════════════════════════════

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class IgnoreTagsBenchmark
    {
        private string testInput;
        private RichTextParserSetting settingNoIgnore;
        private RichTextParserSetting settingWithIgnore;
        private RichTextParser parser;
        private int sink;

        [GlobalSetup]
        public void Setup()
        {
            parser = new RichTextParser();

            var sb = new StringBuilder();
            for (int i = 0; i < 20; i++)
            {
                sb.Append($"<color=red><b><i><size=20>Block {i}</size></i></b></color> text\\n");
            }
            testInput = sb.ToString();

            settingNoIgnore = new RichTextParserSetting(
                TextMeshStyle.Default,
                Array.Empty<Tuple<string, IHintParameter>>(),
                Array.Empty<string>(),
                new HashSet<string>());

            settingWithIgnore = new RichTextParserSetting(
                TextMeshStyle.Default,
                Array.Empty<Tuple<string, IHintParameter>>(),
                Array.Empty<string>(),
                new HashSet<string>
                {
                    "a", "allcaps", "alpha", "b", "color", "font", "font-weight", "gradient",
                    "i", "lowercase", "mark", "noparse", "s", "smallcaps", "style", "sub",
                    "sup", "u", "uppercase", "link"
                });
        }

        [Benchmark(Baseline = true)]
        public void NoIgnoreTags()
        {
            var result = parser.ParseText(testInput, settingNoIgnore);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void WithIgnoreTags()
        {
            var result = parser.ParseText(testInput, settingWithIgnore);
            sink = result.LineInfos.Length;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 7. Parser reuse vs new instance — measure the actual benefit of object pooling
    // ═══════════════════════════════════════════════════════════════════════════

    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class PoolEffectBenchmark
    {
        private string testInput;
        private RichTextParserSetting setting;
        private int sink;

        [GlobalSetup]
        public void Setup()
        {
            setting = new RichTextParserSetting(
                TextMeshStyle.Default,
                Array.Empty<Tuple<string, IHintParameter>>(),
                Array.Empty<string>(),
                new HashSet<string>());

            var sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
                sb.Append($"<color=red><b>Block {i}</b></color> text\\n");
            testInput = sb.ToString();
        }

        [Benchmark(Baseline = true)]
        public void NewParserEachTime()
        {
            var parser = new RichTextParser();
            var result = parser.ParseText(testInput, setting);
            sink = result.LineInfos.Length;
        }

        [Benchmark]
        public void PooledParser()
        {
            var parser = RichTextParserPool.Instance.Rent();
            var result = parser.ParseText(testInput, setting);
            sink = result.LineInfos.Length;
            RichTextParserPool.Instance.Return(parser);
        }
    }
}