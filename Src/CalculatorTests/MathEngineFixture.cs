using MathEngine.Algorithms;
using MathEngine.Records;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathEngineTests
{
    [TestFixture]
    public class MathEngineFixture
    {
        [Test]
        public void CalculateExpression()
        {
            var queue = new Queue<CunkExpression>();
            ShuntingYardAlgorithm.ToRVN("1 + 1".AsSpan(), queue);

            var builder = new StringBuilder();
            buildString();
            Assert.That(builder.ToString(), Is.EqualTo("11+"));

            ShuntingYardAlgorithm.ToRVN("3 + 4 * 2 / (1 - 5)".AsSpan(), queue);
            buildString();
            Assert.That(builder.ToString(), Is.EqualTo("342*15-/+"));

            ShuntingYardAlgorithm.ToRVN("sin ( (3 + 3)  / 6   )".AsSpan(), queue);
            buildString();
            Assert.That(builder.ToString(), Is.EqualTo("33+6/sin"));

            void buildString()
            {
                builder.Clear();
                while (queue.TryDequeue(out var chunk))
                {
                    if(chunk.Item.ChunkType == MathEngine.Enums.ChunkType.Number)
                    {
                        builder.Append(chunk.MemoryOwner.Memory.Span.Slice(0, chunk.PayloadSize));
                        chunk.MemoryOwner.Dispose();
                    }
                    else if(chunk.Item is Operator @operator)
                    {
                        builder.Append(@operator.Pattern);
                    }
                }
            }
        }
    }
}