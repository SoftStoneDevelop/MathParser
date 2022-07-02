using NUnit.Framework;
using System;

namespace MathEngineTests
{
    [TestFixture]
    internal class MathEngineFixture
    {
        [Test]
        public void CalculateExpression()
        {
            var engine = new MathEngine.MathEngineSIMD();
            Assert.That(engine.CalculateExpression("10+10+10+10+10+10+10+10+10+10+10"), Is.EqualTo(110));
            Assert.That(engine.CalculateExpression("1+2-3"), Is.EqualTo(0));
            Assert.That(engine.CalculateExpression("2*(2-3)"), Is.EqualTo(-2));
            Assert.That(engine.CalculateExpression("2 + 3 * 4 / 2 "), Is.EqualTo(8));
            Assert.That(engine.CalculateExpression("1 + 3 * 5 / 12 "), Is.EqualTo(2.25f));

            Assert.That(engine.CalculateExpression("sin(30)"), Is.EqualTo(-0.98803162409f));
            Assert.That(engine.CalculateExpression("sin(45)"), Is.EqualTo(0.85090352453f));

            Assert.That(engine.CalculateExpression("1 + sin(45)"), Is.EqualTo(1.85090352453f));
        }

        [Test]
        public void CalculateExpressionSpan()
        {
            var engine = new MathEngine.MathEngineSIMD();
            Assert.That(engine.CalculateExpression("1+2-3".AsSpan()), Is.EqualTo(0));
            Assert.That(engine.CalculateExpression("2*(2-3)".AsSpan()), Is.EqualTo(-2));
            Assert.That(engine.CalculateExpression("2 + 3 * 4 / 2 ".AsSpan()), Is.EqualTo(8));
            Assert.That(engine.CalculateExpression("1 + 3 * 5 / 12 ".AsSpan()), Is.EqualTo(2.25f));

            Assert.That(engine.CalculateExpression("sin(30)".AsSpan()), Is.EqualTo(-0.98803162409f));
            Assert.That(engine.CalculateExpression("sin(45)".AsSpan()), Is.EqualTo(0.85090352453f));

            Assert.That(engine.CalculateExpression("1 + sin(45)".AsSpan()), Is.EqualTo(1.85090352453f));
        }

        [Test]
        public void Multiplication()
        {
            var engine = new MathEngine.MathEngineSIMD();
            var data = new float[]
            {
                0.987f, 4f, 2, -0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                0.987f, -4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                3.456f
            };
            //Assert.That(engine.Multiplication(data), Is.EqualTo(0.661338389f));

            //data = new float[]
            //{
            //    0.987f
            //};
            //Assert.That(engine.Multiplication(data), Is.EqualTo(0.987f));

            //data = new float[]
            //{
            //    0.987f, 4f, 2, -0.1f,
            //    0.987f, 4f, 2, 0.1f,
            //    -0.987f, 4f, 2, 0.1f,
            //    0.987f, 4f, 2, 0.1f,
            //    0.987f, -4f, 2, 0.1f,
            //    0.987f, 4f, 2, 0.1f,
            //    -0.987f, 4f, 2, 0.1f,
            //    3.456f, 0
            //};

            //Assert.That(engine.Multiplication(data), Is.EqualTo(0f));

            data = new float[]
            {
                0.987f, 4f, 2, -0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                0.987f, -4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                3.456f, 0.987f, 4f, 2, 
                -0.1f, 0.987f, 4f, 2,
                0.1f, -0.987f, 4f, 2,
                0.1f, 0.987f, 4f, 2,
                0.1f, 0.987f, -4f, 2,
                0.1f, 0.987f, 4f, 2,
                0.1f, -0.987f, 4f, 2, 
                0.1f, 3.456f,
            };

            Assert.That(engine.Multiplication(data), Is.EqualTo(0.437368453f));
        }

        [Test]
        public void Addition()
        {
            var engine = new MathEngine.MathEngineSIMD();
            var data = new float[]
            {
                0.987f, 4f, 2, -0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                0.987f, -4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                3.456f
            };
            Assert.That(engine.Addition(data), Is.EqualTo(0.661338389f));

            data = new float[]
            {
                0.987f
            };
            Assert.That(engine.Addition(data), Is.EqualTo(0.987f));

            data = new float[]
            {
                0.987f, 4f, 2, -0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                0.987f, -4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                3.456f, 0
            };

            Assert.That(engine.Addition(data), Is.EqualTo(0f));

            data = new float[]
            {
                0.987f, 4f, 2, -0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                0.987f, -4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                3.456f, 0.987f, 4f, 2,
                -0.1f, 0.987f, 4f, 2,
                0.1f, -0.987f, 4f, 2,
                0.1f, 0.987f, 4f, 2,
                0.1f, 0.987f, -4f, 2,
                0.1f, 0.987f, 4f, 2,
                0.1f, -0.987f, 4f, 2,
                0.1f, 3.456f,
            };

            Assert.That(engine.Addition(data), Is.EqualTo(0f));

            data = new float[]
            {
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 2, 0f,
                0.987f, -4f, 2, 0.1f,
                0.987f, 4f, 2, 0.1f,
                -0.987f, 4f, 2, 0.1f,
                3.456f, 0.987f, 4f, 2,
                -0.1f, 0.987f
            };

            Assert.That(engine.Addition(data), Is.EqualTo(0f));
        }
    }
}