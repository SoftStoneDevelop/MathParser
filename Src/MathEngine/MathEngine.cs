using MathEngine.Algorithms;
using MathEngine.Records;
using System.Runtime.Intrinsics.X86;

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

        public float Multiplication(Span<float> data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentException("Empty data");
            }

            unsafe
            {
                fixed (float* pData = &data[0])
                {
                    int remaining = 0;
                    int payloadSize = data.Length;

                    if(Avx.IsSupported)
                    {
                        remaining = data.Length % 16;
                        payloadSize = data.Length - remaining;
                    }
                    else if (Sse.IsSupported)
                    {
                        remaining = data.Length % 8;
                        payloadSize = data.Length - remaining;
                    }

                    if (Avx.IsSupported)
                    {
                        while (payloadSize >= 16)
                        {
                            int indexResult = 0;
                            for (int i = 0; i < payloadSize; i = i + 16)
                            {
                                var v1 = Avx.LoadVector256(&pData[i]);
                                var v2 = Avx.LoadVector256(&pData[i + 8]);
                                Avx.Store(&pData[indexResult], Avx.Multiply(v1, v2));

                                indexResult += 8;
                            }

                            payloadSize -= indexResult;
                        }
                    }

                    if (Sse.IsSupported)
                    {
                        while (payloadSize >= 8)
                        {
                            int indexResult = 0;
                            for (int i = 0; i < payloadSize; i = i + 8)
                            {
                                var v1 = Sse.LoadVector128(&pData[i]);
                                var v2 = Sse.LoadVector128(&pData[i + 4]);
                                Sse.Store(&pData[indexResult], Sse.Multiply(v1, v2));

                                indexResult += 4;
                            }

                            payloadSize -= indexResult;
                        }
                    }

                    var result = pData[0];
                    for (int i = 1; i < payloadSize; i++)
                    {
                        result *= pData[i];
                    }
                    
                    var startIndx = data.Length - remaining;
                    if (data.Length - remaining == 0)
                    {
                        startIndx++;
                    }

                    for (; startIndx < data.Length; startIndx++)
                    {
                        result *= pData[startIndx];
                    }

                    return result;
                }
            }
        }

        public float Addition(ReadOnlySpan<float> data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentException("Empty data");
            }

            unsafe
            {
                fixed (float* pData = &data[0])
                {
                    int remaining = 0;
                    int payloadSize = data.Length;

                    if (Avx.IsSupported)
                    {
                        remaining = data.Length % 16;
                        payloadSize = data.Length - remaining;
                    }
                    else if (Sse.IsSupported)
                    {
                        remaining = data.Length % 8;
                        payloadSize = data.Length - remaining;
                    }

                    if (Avx.IsSupported)
                    {
                        while (payloadSize >= 16)
                        {
                            int indexResult = 0;
                            for (int i = 0; i < payloadSize; i = i + 16)
                            {
                                var v1 = Avx.LoadVector256(&pData[i]);
                                var v2 = Avx.LoadVector256(&pData[i + 8]);
                                Avx.Store(&pData[indexResult], Avx.Add(v1, v2));

                                indexResult += 8;
                            }

                            payloadSize -= indexResult;
                        }
                    }

                    if (Sse.IsSupported)
                    {
                        while (payloadSize >= 8)
                        {
                            int indexResult = 0;
                            for (int i = 0; i < payloadSize; i = i + 8)
                            {
                                var v1 = Sse.LoadVector128(&pData[i]);
                                var v2 = Sse.LoadVector128(&pData[i + 4]);
                                Sse.Store(&pData[indexResult], Sse.Add(v1, v2));

                                indexResult += 4;
                            }

                            payloadSize -= indexResult;
                        }
                    }

                    var result = pData[0];
                    for (int i = 1; i < payloadSize; i++)
                    {
                        result += pData[i];
                    }

                    var startIndx = data.Length - remaining;
                    if(data.Length - remaining == 0)
                    {
                        startIndx++;
                    }

                    for (; startIndx < data.Length; startIndx++)
                    {
                        result += pData[startIndx];
                    }

                    return result;
                }
            }
        }
    }
}