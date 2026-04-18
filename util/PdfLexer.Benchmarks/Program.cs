using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using PdfLexer.Benchmarks.Benchmarks;
using System;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PdfLexer.Benchmarks
{
    internal class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddDiagnoser(MemoryDiagnoser.Default);
            // AddJob(Job.ShortRun.WithWarmupCount(1).WithIterationCount(5));
            AddJob(Job.ShortRun.WithWarmupCount(5).WithIterationCount(25));
            // AddJob(Job.ShortRun.WithWarmupCount(25).WithIterationCount(100));
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && string.Equals(args[0], "--textlayout-baseline", StringComparison.Ordinal))
            {
                RunTextLayoutBaseline();
                return;
            }

            BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()).Run(args);
        }

        private static void RunTextLayoutBaseline()
        {
            using var benchmarks = new TextLayoutBenchmarks();
            benchmarks.Setup();

            for (var i = 0;i < 10000; i++)
            {
                benchmarks.FlatLayout_LongParagraph();
            }

            Console.WriteLine("TextLayout baseline (manual runner)");
            RunScenario("FlatLayout_LongParagraph", benchmarks.FlatLayout_LongParagraph);
            RunScenario("RichAnalyzeFit_NestedContent", benchmarks.RichAnalyzeFit_NestedContent);
            RunScenario("RichWrite_PrecomputedPlan", benchmarks.RichWrite_PrecomputedPlan);
        }

        private static void RunScenario<T>(string name, Func<T> action)
        {
            for (var i = 0; i < 5; i++)
            {
                action();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            const int iterations = 50;
            long allocatedBytes = 0;
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                var before = GC.GetAllocatedBytesForCurrentThread();
                _ = action();
                allocatedBytes += GC.GetAllocatedBytesForCurrentThread() - before;
            }

            stopwatch.Stop();
            Console.WriteLine($"{name}: mean={stopwatch.Elapsed.TotalMilliseconds / iterations:F3} ms, alloc={allocatedBytes / iterations} B/op");
        }
    }
}
