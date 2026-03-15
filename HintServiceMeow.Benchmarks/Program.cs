using System;
using BenchmarkDotNet.Running;
using HintServiceMeow.Benchmarks;
using HintServiceMeow.Benchmarks.Benchmarks;
using HintServiceMeow.Core.Utilities.Tools;

namespace MyProject.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Instance = new TestLogger();

            Console.WriteLine("=== RichTextParser Performance Profiling ===");
            Console.WriteLine();
            Console.WriteLine("  1. PhaseBenchmark          - Tokenize vs Parse stage breakdown");
            Console.WriteLine("  2. InputProfileBenchmark   - By input type (plain/tags/long/lines/nested)");
            Console.WriteLine("  3. ComponentBenchmark      - Micro benchmarks (TagChecker/FontTool/Pool)");
            Console.WriteLine("  4. AllocationBenchmark     - Memory allocation hotspot analysis");
            Console.WriteLine("  5. ScalabilityBenchmark    - Scaling curve (1/5/10/50/100)");
            Console.WriteLine("  6. IgnoreTagsBenchmark     - IgnoreTags overhead comparison");
            Console.WriteLine("  7. PoolEffectBenchmark     - Object pool vs new instance");
            Console.WriteLine("  8. HintParserBenchmark     - Hint Parser Performance");
            Console.WriteLine("  9. Run ALL (takes a while)");
            Console.WriteLine();
            Console.Write("Select (1-8): ");

            string choice = Console.ReadLine()?.Trim() ?? "1";

            switch (choice)
            {
                case "1":
                    BenchmarkRunner.Run<PhaseBenchmark>();
                    break;
                case "2":
                    BenchmarkRunner.Run<InputProfileBenchmark>();
                    break;
                case "3":
                    BenchmarkRunner.Run<ComponentBenchmark>();
                    break;
                case "4":
                    BenchmarkRunner.Run<AllocationBenchmark>();
                    break;
                case "5":
                    BenchmarkRunner.Run<ScalabilityBenchmark>();
                    break;
                case "6":
                    BenchmarkRunner.Run<IgnoreTagsBenchmark>();
                    break;
                case "7":
                    BenchmarkRunner.Run<PoolEffectBenchmark>();
                    break;
                case "8":
                    BenchmarkRunner.Run<HintParserBenchmark>();
                    break;
                case "9":
                    BenchmarkRunner.Run<PhaseBenchmark>();
                    BenchmarkRunner.Run<InputProfileBenchmark>();
                    BenchmarkRunner.Run<ComponentBenchmark>();
                    BenchmarkRunner.Run<AllocationBenchmark>();
                    BenchmarkRunner.Run<ScalabilityBenchmark>();
                    BenchmarkRunner.Run<IgnoreTagsBenchmark>();
                    BenchmarkRunner.Run<PoolEffectBenchmark>();
                    break;
                default:
                    Console.WriteLine("Invalid choice, running PhaseBenchmark");
                    BenchmarkRunner.Run<PhaseBenchmark>();
                    break;
            }

            Console.WriteLine("Benchmarks complete. Press any key to exit...");
            Console.ReadKey();
        }
    }
}