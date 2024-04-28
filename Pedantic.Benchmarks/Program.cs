using BenchmarkDotNet.Running;

namespace Pedantic.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<MethodCallBenchmarks>();
            var summary = BenchmarkRunner.Run<RandomAccessMemoryBenchmarks>();
        }
    }
}
