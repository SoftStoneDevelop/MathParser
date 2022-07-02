using MathEngine.Algorithms;
using MathEngine.Records;
using System.Runtime.Intrinsics.X86;

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

        public float Multiplication(Span<float> data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentException("Empty data");
            }

            float result;
            if (data.Length < 8)
            {
                result = data[0];
                for (int i = 1; i < data.Length; i++)
                {
                    result *= data[i];
                }

                return result;
            }

            if (Sse.IsSupported)
            {
                unsafe
                {
                    fixed (float* pData = &data[0])
                    {
                        var remaining = data.Length % 8;
                        var remainingData = data.Slice(data.Length - remaining, remaining);

                        int payloadSize = data.Length - remaining;
                        while (payloadSize >= 8)
                        {
                            int indexResult = 0;
                            int endDataIndex = payloadSize;
                            for (int i = 0; i < endDataIndex; i = i + 8)
                            {
                                var v1 = Sse.LoadVector128(&pData[i]);
                                var v2 = Sse.LoadVector128(&pData[i + 4]);
                                Sse.Store(&pData[indexResult], Sse.Multiply(v1, v2));

                                indexResult += 4;
                            }

                            payloadSize -= indexResult;
                        }

                        result = data[0];
                        for (int i = 1; i < payloadSize; i++)
                        {
                            result *= data[i];
                        }

                        for (int i = 0; i < remainingData.Length; i++)
                        {
                            result *= remainingData[i];
                        }

                        return result;
                    }
                }
            }

            result = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                result *= data[i];
            }

            return result;
        }

        public float Addition(ReadOnlySpan<float> data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentException("Empty data");
            }

            float result;
            if (data.Length < 4)
            {
                result = data[0];
                for (int i = 1; i < data.Length; i++)
                {
                    result += data[i];
                }

                return result;
            }

            //if (Sse.IsSupported)
            //{
            //    unsafe
            //    {
            //        fixed (float* pData = &data[0])
            //        {
            //            var remaining = data.Length % 8;
            //            var remainingData = data.Slice(data.Length - remaining, remaining);

            //            int payloadSize = data.Length - remaining;
            //            while (payloadSize > 4)
            //            {
            //                int indexResult = 0;
            //                int endDataIndex = payloadSize;
            //                for (int i = 0; i < endDataIndex - 4; i += 8)
            //                {
            //                    var v1 = Sse.LoadVector128(&pData[i]);
            //                    var v2 = Sse.LoadVector128(&pData[i + 4]);
            //                    Sse.Store(&pData[indexResult], Sse.Add(v1, v2));

            //                    indexResult += 4;
            //                }

            //                payloadSize -= indexResult;
            //            }

            //            result = pData[0];
            //            for (int i = 1; i < payloadSize; i++)
            //            {
            //                result += data[i];
            //            }

            //            for (int i = 0; i < remainingData.Length; i++)
            //            {
            //                result += remainingData[i];
            //            }

            //            return result;
            //        }
            //    }
            //}

            result = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                result += data[i];
            }

            return result;
        }

        public float Subtraction(ReadOnlySpan<float> data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentException("Empty data");
            }

            float result;
            if (data.Length < 4)
            {
                result = data[0];
                for (int i = 1; i < data.Length; i++)
                {
                    result -= data[i];
                }

                return result;
            }

            if (Sse.IsSupported)
            {
                unsafe
                {
                    fixed (float* pData = &data[0])
                    {
                        var remaining = data.Length % 8;
                        var remainingData = data.Slice(data.Length - remaining, remaining);

                        int payloadSize = data.Length - remaining;
                        while (payloadSize > 4)
                        {
                            int indexResult = 0;
                            int endDataIndex = payloadSize;
                            for (int i = 0; i < endDataIndex - 4; i += 8)
                            {
                                var v1 = Sse.LoadVector128(&pData[i]);
                                var v2 = Sse.LoadVector128(&pData[i + 4]);
                                Sse.Store(&pData[indexResult], Sse.Subtract(v1, v2));

                                indexResult += 4;
                            }

                            payloadSize -= indexResult;
                        }

                        result = pData[0];
                        for (int i = 1; i < payloadSize; i++)
                        {
                            result -= data[i];
                        }

                        for (int i = 0; i < remainingData.Length; i++)
                        {
                            result -= remainingData[i];
                        }

                        return result;
                    }
                }
            }

            result = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                result -= data[i];
            }

            return result;
        }
    }
}