using MathEngine.Algorithms;
using MathEngine.Helpers;
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
        static IEnumerable<object[]> Source()
        {
            yield return new object[]
            {
                "1 + 1",
                new ChunkExpression[]
                {
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Addition),
                },
                "1 1 +"
            };

            yield return new object[]
            {
                "3 + 4 * 2 / (1 - 5)",
                new ChunkExpression[]
                {
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new ChunkNumber(4, ParserHelper.NumberOperand),
                    new ChunkNumber(2, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Multiplication),
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new ChunkNumber(5, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Subtraction),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkExpression(ParserHelper.Addition),
                },
                "3 4 2 * 1 5 - / +"
            };

            yield return new object[]
            {
                "sin ( (3 + 3)  / 6   )",
                new ChunkExpression[]
                {
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Addition),
                    new ChunkNumber(6, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkExpression(ParserHelper.Sin),
                },
                "3 3 + 6 / sin"
            };

            yield return new object[]
           {
                "1 + sin(45)",
                new ChunkExpression[]
                {
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new ChunkNumber(45, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Sin),
                    new ChunkExpression(ParserHelper.Addition),
                },
                "1 45 sin +"
           };
        }

        [TestCaseSource(nameof(Source))]
        public void ToRVN(object[] testData)
        {
            var queueExprect = new Queue<ChunkExpression>(
                (ChunkExpression[])testData[1]
                );
            var queue = new Queue<ChunkExpression>();
            var builder = new StringBuilder();
            ShuntingYardAlgorithm.ToRVN(((string)testData[0]).AsSpan(), queue, builder);
            Assert.That(builder.ToString(), Is.EqualTo((string)testData[2]));
            builder.Clear();
            while(queue.TryDequeue(out var chunk))
            {
                Assert.That(queueExprect.Dequeue(), Is.EqualTo(chunk));
            }
        }
    }
}