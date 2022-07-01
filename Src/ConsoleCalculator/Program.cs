using CalculatorEngine;
using System;
using System.Runtime.Intrinsics.X86;

namespace ConsoleCalculator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var c = new JobClass();
            c.Job();
            Console.ReadLine();
        }

        
    }

    public class JobClass
    {
        private CalculatorSIMD _class = new();
        private int _iterations = 100000;

        public void Job()
        {
            for (int i = 0; i < _iterations; i++)
            {
                //_class.Addition(i, _iterations);
            }
        }
    }
}