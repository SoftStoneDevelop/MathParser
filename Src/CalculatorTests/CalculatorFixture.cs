using CalculatorEngine;
using NUnit.Framework;

namespace CalculatorTests
{
    [TestFixture]
    public class CalculatorFixture
    {
        private ICalculator _calculator;
        
        [SetUp]
        public void SetUp()
        {
            _calculator = new Calculator();
        }

        [TestCase("1 + 1", ExpectedResult = 2f)]
        //[TestCase("5 * 3 + 2", ExpectedResult = 17f)]
        //[TestCase("5 + 3 * 2", ExpectedResult = 17f)]
        public float Addition(string expression)
        {
            return _calculator.CalculateExpression(expression);
        }
    }
}