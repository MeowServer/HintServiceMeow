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

            // Start the Benchmark Runner
            Console.WriteLine("Which benchmark would you like to run?");
            Console.WriteLine("1 : HintParserBenchmark");
            Console.WriteLine("2 : HintCollectionBenchmark");

            switch (Console.ReadLine())
            {
                case "1":
                    _ = BenchmarkRunner.Run<HintParserBenchmark>();
                    break;
                case "2":
                    _ = BenchmarkRunner.Run<HintCollectionBenchmark>();
                    break;
                default:
                    Console.WriteLine("Invalid selection. Exiting...");
                    break;
            }

            Console.WriteLine("Benchmarks complete. Press any key to exit...");
            Console.ReadKey();
        }
    }
}