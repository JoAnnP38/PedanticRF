using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Pedantic.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MethodCallBenchmarks>();
        }
    }
}
