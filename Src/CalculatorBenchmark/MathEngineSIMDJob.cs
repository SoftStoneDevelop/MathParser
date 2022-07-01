using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MathEngine;

namespace MathEngineBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class MathEngineSIMDJob
    {
        [Params(100, 1000, 10000, 100000, 250000, 500000, 1000000)]
        public int Iterations;

        private ICalculator _calculator;

        [GlobalSetup]
        public void Setup()
        {
            _calculator = new CalculatorSIMD();
        }

        [Benchmark]
        public void AddCalculatorSMID()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _calculator.CalculateExpression("");
            }
        }
    }
}
