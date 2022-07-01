using BenchmarkDotNet.Running;

namespace MathEngineBenchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MathEngineJob>();
            BenchmarkRunner.Run<MathEngineSIMDJob>();
        }
    }
}