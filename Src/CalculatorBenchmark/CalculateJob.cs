using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CalculatorEngine;

namespace CalculatorBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class CalculateJob
    {
        [Params(100, 1000, 10000, 100000, 250000, 500000, 1000000)]
        public int Iterations;

        private ICalculator _calculator;

        [GlobalSetup]
        public void Setup()
        {
            _calculator = new Calculator();
        }

        [Benchmark]
        public void Calculate()
        {
            for (int i = 0; i < Iterations; i++)
            {
                _calculator.CalculateExpression("");
            }
        }
    }
}