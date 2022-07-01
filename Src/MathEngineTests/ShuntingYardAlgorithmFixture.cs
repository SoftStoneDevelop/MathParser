using MathEngine.Algorithms;
using MathEngine.Records;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathEngineTests
{
    [TestFixture]
    internal class ShuntingYardAlgorithmFixture
    {
        [Test]
        public void ToRVN()
        {
            var queue = new Queue<ChunkExpression>();
            var builder = new StringBuilder();
            ShuntingYardAlgorithm.ToRVN("1 + 1".AsSpan(), queue, builder);
            Assert.That(builder.ToString(), Is.EqualTo("1 1 +"));
            builder.Clear();

            ShuntingYardAlgorithm.ToRVN("3 + 4 * 2 / (1 - 5)".AsSpan(), queue, builder);
            Assert.That(builder.ToString(), Is.EqualTo("3 4 2 * 1 5 - / +"));
            builder.Clear();

            ShuntingYardAlgorithm.ToRVN("sin ( (3 + 3)  / 6   )".AsSpan(), queue, builder);
            Assert.That(builder.ToString(), Is.EqualTo("3 3 + 6 / sin"));
            builder.Clear();

            ShuntingYardAlgorithm.ToRVN("1 + sin(45)".AsSpan(), queue, builder);
            Assert.That(builder.ToString(), Is.EqualTo("1 45 sin +"));
        }
    }
}