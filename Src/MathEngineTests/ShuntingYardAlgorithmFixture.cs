using MathEngine.Algorithms;
using MathEngine.Helpers;
using MathEngine.Records;
using NUnit.Framework;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace MathEngineTests
{
    [TestFixture]
    internal class ShuntingYardAlgorithmFixture
    {
        static IEnumerable<object[]> Source()
        {
            var pool = MemoryPool<float>.Shared;

            var memory = pool.Rent(2);
            var data = new float[]
            {
                0f,
                1f
            };

            FillMemory1();

            yield return new object[]
            {
                "1 + 1",
                new ChunkExpression[]
                {
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(memory, 1, 2, ParserHelper.Addition),
                },
                "1 1 S+(2) "
            };

            memory = pool.Rent(2);
            data = new float[]
            {
                0f,
                2f
            };

            FillMemory1();

            var memory2 = pool.Rent(1);
            data = new float[]
            {
                0f,
                5f
            };

            FillMemory2();

            yield return new object[]
            {
                "3 + 4 * 2 / (1 - 5)",
                new ChunkExpression[]
                {
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new ChunkNumber(4, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(memory, 1, 2, ParserHelper.Multiplication),
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(memory2, 1, 2, ParserHelper.Subtraction),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkExpression(ParserHelper.Addition),
                },
                "3 4 2 S*(2) 1 5 S-(2) / + "
            };

            memory = pool.Rent(2);
            data = new float[]
            {
                0f,
                3f
            };

            FillMemory1();

            yield return new object[]
            {
                "sin ( (3 + 3)  / 6   )",
                new ChunkExpression[]
                {
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(memory, 1, 2, ParserHelper.Addition),
                    new ChunkNumber(6, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkExpression(ParserHelper.Sin),
                },
                "3 3 S+(2) 6 / sin "
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
                "1 45 sin + "
            };

            memory = pool.Rent(2);
            data = new float[]
            {
                0f,
                4f,
                3f,
                2f
            };

            FillMemory1();

            yield return new object[]
            {
                "1 + 2 + 3 + 4 + 5 / 16 / 5 / 3",
                new ChunkExpression[]
                {
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(memory, 3, 4, ParserHelper.Addition),
                    new ChunkNumber(5, ParserHelper.NumberOperand),
                    new ChunkNumber(16, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkNumber(5, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkExpression(ParserHelper.Addition),
                },
                "1 4 3 2 S+(4) 5 16 / 5 / 3 / + "
            };

            void FillMemory1()
            {
                var index = 0;
                foreach (var item in data)
                {
                    memory.Memory.Span[index] = item;
                    index++;
                }
            }

            void FillMemory2()
            {
                var index = 0;
                foreach (var item in data)
                {
                    memory2.Memory.Span[index] = item;
                    index++;
                }
            }
        }

        [Test]
        public void ToRVNIncorrectInput()
        {
            Assert.That(
                () => ShuntingYardAlgorithm.ToRVNOpt(")3 + (1 - 5)".AsSpan(), new Queue<ChunkExpression>()),
                Throws.Exception.TypeOf(typeof(InvalidOperationException)).And
                .Message.EqualTo("The operator stack is empty"));

            Assert.That(
                () => ShuntingYardAlgorithm.ToRVNOpt("3 + 1 - 5)".AsSpan(), new Queue<ChunkExpression>()),
                Throws.Exception.TypeOf(typeof(InvalidOperationException)).And
                .Message.EqualTo("There are mismatched parentheses 'Left Bracket'"));

            Assert.That(
                () => ShuntingYardAlgorithm.ToRVNOpt("3 + 1 - 5(".AsSpan(), new Queue<ChunkExpression>()),
                Throws.Exception.TypeOf(typeof(InvalidOperationException)).And
                .Message.EqualTo("There are mismatched parentheses 'Left Bracket in end'"));
        }

        [TestCaseSource(nameof(Source))]
        public void ToRVN(object[] testData)
        {
            var queueExprect = new Queue<ChunkExpression>(
                (ChunkExpression[])testData[1]
                );
            var queue = new Queue<ChunkExpression>();
            ShuntingYardAlgorithm.ToRVNOpt(((string)testData[0]).AsSpan(), queue);
            while(queue.TryDequeue(out var chunk))
            {
                var expectedResut = queueExprect.Dequeue();

                Assert.That(expectedResut, Is.TypeOf(chunk.GetType()));
                switch (expectedResut)
                {
                    case SequenceNumberOperation sequenceExp:
                    {
                        var sequence = (SequenceNumberOperation)chunk;
                        Assert.That(sequenceExp.ExpectedParamsCount, Is.EqualTo(sequence.ExpectedParamsCount));
                        Assert.That(sequenceExp.Item, Is.EqualTo(sequence.Item));
                        Assert.That(sequenceExp.PatternItem, Is.EqualTo(sequence.PatternItem));
                        Assert.That(sequenceExp.Size, Is.EqualTo(sequence.Size));
                        Assert.That(
                            sequenceExp.SequenceMemory.Memory.Span.Slice(0, sequenceExp.ExpectedParamsCount).ToArray(),
                            Is.EquivalentTo(sequence.SequenceMemory.Memory.Span.Slice(0, sequence.ExpectedParamsCount).ToArray())
                            );

                        sequence.SequenceMemory.Dispose();
                        sequenceExp.SequenceMemory.Dispose();

                        break;
                    }

                    case ChunkNumber chunkNumberExp:
                    {
                        var chunkNumber = (ChunkNumber)chunk;
                        Assert.That(chunkNumberExp, Is.EqualTo(chunkNumber));
                        break;
                    }

                    case ChunkExpression:
                    {
                        Assert.That(expectedResut, Is.EqualTo(chunk));
                        break;
                    }
                }
            }
        }

        [TestCaseSource(nameof(Source))]
        public void ToRVNPrint(object[] testData)
        {
            var queue = new Queue<ChunkExpression>();
            ShuntingYardAlgorithm.ToRVNOpt(((string)testData[0]).AsSpan(), queue);

            var builder = new StringBuilder();
            ShuntingYardAlgorithm.PrintRVNAndDequeueAll(queue, builder);
            Assert.That(builder.ToString(), Is.EqualTo((string)testData[2]));
        }
    }
}