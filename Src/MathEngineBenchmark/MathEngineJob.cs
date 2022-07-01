using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MathEngine;

namespace MathEngineBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class MathEngineJob
    {
        [Params(100, 1000, 10000, 100000, 250000, 500000, 1000000)]
        public int Iterations;

        private IMathEngine _mathEngine;
        private IMathEngine _mathEngineSIMD;

        [GlobalSetup]
        public void Setup()
        {
            _mathEngine = new MathEngine.MathEngine();
            _mathEngineSIMD = new MathEngine.MathEngineSIMD();
        }

        [Benchmark]
        public void Calculate()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _mathEngine.CalculateExpression("");
            }
        }

        [Benchmark(Baseline = true, Description = "With SIMD operation uses")]
        public void CalculateSIMD()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _mathEngineSIMD.CalculateExpression("");
            }
        }
    }
}