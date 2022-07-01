using BenchmarkDotNet.Running;

namespace MathEngineBenchmark
{
    internal class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<MathEngineJob>();
        }
    }
}