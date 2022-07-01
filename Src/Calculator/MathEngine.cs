using MathEngine.Algorithms;
using MathEngine.Records;
using System.Globalization;

namespace MathEngine
{
    public class MathEngine : IMathEngine
    {
        public float CalculateExpression(string expression)
        {
            return CalculateExpression(expression.AsSpan());
        }

        public float CalculateExpression(ReadOnlySpan<char> expression)
        {
            var queue = new Queue<ChunkExpression>();
            ShuntingYardAlgorithm.ToRVN(expression, queue);

            try
            {
                var stackOperands = new Stack<float>();
                while (queue.TryDequeue(out var chunk))
                {
                    if (chunk.Item.ChunkType == Enums.ChunkType.Number)
                    {
                        if (!float.TryParse(
                            chunk.MemoryOwner.Memory.Span.Slice(0, chunk.PayloadSize),
                            NumberStyles.Any,
                            CultureInfo.CurrentCulture,//TODO replace on correct for ',' and '.' separators
                            out var number)
                            )
                        {
                            throw new ArgumentException($"Invalid number '{chunk.MemoryOwner.Memory.Span.Slice(0, chunk.PayloadSize)}'");
                        }

                        chunk.MemoryOwner.Dispose();
                        stackOperands.Push(number);
                        continue;
                    }

                    if (chunk.Item is Function function)
                    {
                        if (stackOperands.Count < function.ParametrsCount)
                        {
                            throw new ArgumentException($"Not enough operands for calculate operator '{chunk.Item}'");
                        }

                        switch (function.ChunkType)
                        {
                            case Enums.ChunkType.Sin:
                            {
                                var a = stackOperands.Pop();
                                stackOperands.Push((float)Math.Sin(a));//casting can be dangerous TODO safe
                                break;
                            }
                        }

                        continue;
                    }

                    if (chunk.Item is Operator @operator)
                    {
                        if (stackOperands.Count < 2)
                        {
                            throw new ArgumentException($"Not enough operands for calculate operator '{chunk.Item}'");
                        }

                        var b = stackOperands.Pop();
                        var a = stackOperands.Pop();

                        switch (@operator.ChunkType)
                        {
                            case Enums.ChunkType.Multiplication:
                            {
                                stackOperands.Push(a * b);
                                break;
                            }

                            case Enums.ChunkType.Subtraction:
                            {
                                stackOperands.Push(a - b);
                                break;
                            }

                            case Enums.ChunkType.Division:
                            {
                                stackOperands.Push(a / b);
                                break;
                            }

                            case Enums.ChunkType.Addition:
                            {
                                stackOperands.Push(a + b);
                                break;
                            }
                        }
                    }
                }

                return stackOperands.Pop();
            }
            catch
            {
                while (queue.TryDequeue(out var chunk))
                    chunk.MemoryOwner?.Dispose();

                throw;
            }
        }
    }
}