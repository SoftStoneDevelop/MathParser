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
            yield return new object[]
            {
                "1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 * 2 * 2 * 2 * 2 * 2 * 2 * 2 * 2 * 2 ",
                new ChunkExpression[]
                {
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            1f,
                            1f,
                            1f,
                            1f,
                            1f,
                            1f,
                            1f
                        },
                        3
                        )
                        , 7, 8, ParserHelper.Addition),
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            2f,
                            2f,
                            2f,
                            2f,
                            2f,
                            2f,
                            2f,
                            2f,
                            2f
                        },
                        2
                        )
                        , 9, 10, ParserHelper.Multiplication),
                    new ChunkExpression(ParserHelper.Addition)
                },
                "1 1 1 1 1 1 1 1 S+(8) 1 2 2 2 2 2 2 2 2 2 S*(10) + "
            };

            yield return new object[]
            {
                "10 * 10 * 10 + 10 + 10",
                new ChunkExpression[]
                {
                    new ChunkNumber(10, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            10f,
                            10f
                        },
                        3
                        )
                        , 2, 3, ParserHelper.Multiplication),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            10f,
                            10f
                        },
                        2
                        )
                        , 2, 3, ParserHelper.Addition),
                },
                "10 10 10 S*(3) 10 10 S+(3) "
            };

            yield return new object[]
            {
                "0 + 1",
                new ChunkExpression[]
                {
                    new ChunkNumber(0, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            1f
                        },
                        2
                        )
                        , 1, 2, ParserHelper.Addition),
                },
                "0 1 S+(2) "
            };

            yield return new object[]
            {
                "10+10+10+10+10+10+10+10+10+10+10",
                new ChunkExpression[]
                {
                    new ChunkNumber(10, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(FillMemory(new float[]
                        {
                            0f,
                            10f,
                            10f,
                            10f,
                            10f,
                            10f,
                            10f,
                            10f,
                            10f,
                            10f,
                            10f,
                        },
                        11
                        )
                        , 10, 11, ParserHelper.Addition),
                },
                "10 10 10 10 10 10 10 10 10 10 10 S+(11) "
            };

            yield return new object[]
            {
                "1 + 1",
                new ChunkExpression[]
                {
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            1f
                        },
                        2
                        )
                        , 1, 2, ParserHelper.Addition),
                },
                "1 1 S+(2) "
            };

            yield return new object[]
            {
                "3 + 4 * 2 / (1 - 5)",
                new ChunkExpression[]
                {
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new ChunkNumber(4, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            2f
                        },
                        2
                        )
                        , 1, 2, ParserHelper.Multiplication),
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new ChunkNumber(5, ParserHelper.NumberOperand),
                    new ChunkExpression(ParserHelper.Subtraction),
                    new ChunkExpression(ParserHelper.Division),
                    new ChunkExpression(ParserHelper.Addition),
                },
                "3 4 2 S*(2) 1 5 - / + "
            };

            yield return new object[]
            {
                "sin ( (3 + 3)  / 6   )",
                new ChunkExpression[]
                {
                    new ChunkNumber(3, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            3f
                        },
                        2
                        ), 1, 2, ParserHelper.Addition),
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
            
            yield return new object[]
            {
                "1 + 2 + 3 + 4 + 5 / 16 / 5 / 3",
                new ChunkExpression[]
                {
                    new ChunkNumber(1, ParserHelper.NumberOperand),
                    new SequenceNumberOperation(
                        FillMemory(new float[]
                        {
                            0f,
                            4f,
                            3f,
                            2f
                        },
                        4
                        ), 3, 4, ParserHelper.Addition),
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

            IMemoryOwner<float> FillMemory(float[] data, int rentSize)
            {
                var memoryOwner = MemoryPool<float>.Shared.Rent(rentSize);
                var index = 0;
                foreach (var item in data)
                {
                    memoryOwner.Memory.Span[index] = item;
                    index++;
                }

                return memoryOwner;
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