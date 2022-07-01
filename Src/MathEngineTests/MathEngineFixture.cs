using NUnit.Framework;

namespace MathEngineTests
{
    [TestFixture]
    internal class MathEngineFixture
    {
        [Test]
        public void CalculateExpression()
        {
            var engine = new MathEngine.MathEngine();
            Assert.That(engine.CalculateExpression("1+2-3"), Is.EqualTo(0));
            Assert.That(engine.CalculateExpression("2*(2-3)"), Is.EqualTo(-2));
            Assert.That(engine.CalculateExpression("2 + 3 * 4 / 2 "), Is.EqualTo(8));
            Assert.That(engine.CalculateExpression("1 + 3 * 5 / 12 "), Is.EqualTo(2.25f));

            Assert.That(engine.CalculateExpression("sin(30)"), Is.EqualTo(-0.98803162409f));
            Assert.That(engine.CalculateExpression("sin(45)"), Is.EqualTo(0.85090352453f));

            Assert.That(engine.CalculateExpression("1 + sin(45)"), Is.EqualTo(1.85090352453f));
        }
    }
}