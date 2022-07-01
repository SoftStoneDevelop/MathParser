using BenchmarkDotNet.Running;
using System.Diagnostics;

namespace CalculatorBenchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CalculateJob>();
            BenchmarkRunner.Run<CalculateSIMDJob>();
        }
    }
}