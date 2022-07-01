using NUnit.Framework;

namespace MathEngineTests
{
    [TestFixture]
    internal class MethEngineFixture
    {
        [Test]
        public void ToRVN()
        {
            var engine = new MathEngine.MathEngine();
            Assert.That(engine.CalculateExpression("1+2-3"), Is.EqualTo(0));
            Assert.That(engine.CalculateExpression("2*(2-3)"), Is.EqualTo(-2));
            Assert.That(engine.CalculateExpression("2 + 3 * 4 / 2 "), Is.EqualTo(8));
        }
    }
}