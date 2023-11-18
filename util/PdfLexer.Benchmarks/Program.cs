using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using PdfLexer.Benchmarks.Benchmarks;
using System.Reflection;
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
            BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()).Run(args);
        }
    }
}
