using MathEngine.Algorithms;
using MathEngine.Records;

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

            var stackOperands = new Stack<float>();
            while (queue.TryDequeue(out var chunk))
            {
                if (chunk.Item.ChunkType == Enums.ChunkType.Number)
                {
                    stackOperands.Push(((ChunkNumber)chunk).Number);
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
    }
}