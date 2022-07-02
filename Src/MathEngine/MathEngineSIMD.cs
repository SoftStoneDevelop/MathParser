using MathEngine.Algorithms;
using MathEngine.Records;

namespace MathEngine
{
    public class MathEngineSIMD : IMathEngine
    {
        public float CalculateExpression(string expression)
        {
            return CalculateExpression(expression.AsSpan());
        }

        public float CalculateExpression(ReadOnlySpan<char> expression)
        {
            var queue = new Queue<ChunkExpression>();
            ShuntingYardAlgorithm.ToRVNOpt(expression, queue);
            var stackOperands = new Stack<float>();

            while (queue.TryDequeue(out var chunk))
            {
                if (chunk.Item.ChunkType == Enums.ChunkType.Number)
                {
                    stackOperands.Push(((ChunkNumber)chunk).Number);
                    continue;
                }
                else
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
                else
                if(chunk is SequenceNumberOperation sequenceNumberOperation)
                {
                    var startIndex = sequenceNumberOperation.ExpectedParamsCount - sequenceNumberOperation.Size;
                    var spanTemp =
                        sequenceNumberOperation.SequenceMemory.Memory.Span
                        .Slice(0, sequenceNumberOperation.ExpectedParamsCount);

                    for (int i = 0; i < startIndex; i++)
                    {
                        if(!stackOperands.TryPop(out var number))
                        {
                            throw new ArgumentException($"Not enough operands for calculate operator '{chunk.Item}'");
                        }

                        spanTemp[i] = number;
                    }

                    switch (sequenceNumberOperation.Item.ChunkType)
                    {
                        case Enums.ChunkType.Multiplication:
                        {
                            var result = Multiplication(spanTemp);
                            stackOperands.Push(result);
                            break;
                        }

                        case Enums.ChunkType.Addition:
                        {
                            var result = Addition(spanTemp);
                            stackOperands.Push(result);
                            break;
                        }

                        case Enums.ChunkType.Subtraction:
                        {
                            var result = Subtraction(spanTemp);
                            stackOperands.Push(result);
                            break;
                        }

                        default:
                        {
                            throw new NotImplementedException($"Unknown {nameof(SequenceNumberOperation)}: {sequenceNumberOperation}");
                        }
                    }

                    continue;
                }
                else
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

                    continue;
                }
            }

            return stackOperands.Pop();
        }

        public float Multiplication(ReadOnlySpan<float> data)
        {
            //TODO SIMD
            float result = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                result *= data[i];
            }

            return result;
        }

        public float Addition(ReadOnlySpan<float> data)
        {
            //TODO SIMD
            float result = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                result += data[i];
            }

            return result;
        }

        public float Subtraction(ReadOnlySpan<float> data)
        {
            //TODO SIMD
            float result = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                result -= data[i];
            }

            return result;
        }
    }
}