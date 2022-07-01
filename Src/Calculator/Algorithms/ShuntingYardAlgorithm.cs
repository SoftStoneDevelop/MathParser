using MathEngine.Enums;
using MathEngine.Helpers;
using MathEngine.Records;
using System.Buffers;

namespace MathEngine.Algorithms
{
    public static class ShuntingYardAlgorithm
    {
        /// <summary>
        /// Convert infix notation to RPN <see cref="https://en.wikipedia.org/wiki/Shunting_yard_algorithm"/>
        /// </summary>
        /// <param name="chars">Input math expression in infix notation</param>
        /// <param name="output">Output data in the peverse polish notation <see cref="https://en.wikipedia.org/wiki/Reverse_Polish_notation"/></param>
        /// <returns>Real write chars length</returns>
        public static void ToRVN(
            ReadOnlySpan<char> chars,
            Queue<CunkExpression> output
            )
        {
            var spanIterate = chars.Slice(0);
            var stackOperators = new Stack<Operator>();
            var poolMemory = MemoryPool<char>.Shared;

            for (int i = 0; i < spanIterate.Length; i++)
            {
                try
                {
                    if (spanIterate[i] == '(')
                    {
                        stackOperators.Push(ParserHelper.LeftBracket);
                        continue;
                    }

                    if (spanIterate[i] == ')')
                    {
                        while(
                            stackOperators.TryPeek(out var topOperator) &&
                            topOperator != ParserHelper.LeftBracket
                            )
                        {
                            var operatorToOutpit = stackOperators.Pop();
                            var memory = poolMemory.Rent(operatorToOutpit.Pattern.Length);
                            try
                            {
                                var spanMemory = memory.Memory.Span;
                                operatorToOutpit.Pattern.CopyTo(spanMemory);
                                output.Enqueue(new(memory, operatorToOutpit.Pattern.Length, operatorToOutpit.ChunkType));
                            }
                            catch
                            {
                                memory.Dispose();
                                throw;
                            }
                        }

                        if(stackOperators.Count == 0)
                        {
                            throw new InvalidOperationException("Not found left bracket");
                        }
                        stackOperators.Pop();
                    }

                    var numberLength = ParserHelper.IsNumber(spanIterate.Slice(i));
                    if (numberLength != -1)
                    {
                        var memory = poolMemory.Rent(numberLength + 1);
                        try
                        {
                            var spanMemory = memory.Memory.Span;
                            spanIterate.Slice(i, numberLength).CopyTo(spanMemory);
                            output.Enqueue(new(memory, numberLength, ChunkType.Number));
                        }
                        catch
                        {
                            memory.Dispose();
                            throw;
                        }
                    }

                    Operator op = null;
                    for (int j = 0; j < ParserHelper.Operators.Length; j++)
                    {
                        var tempOp = ParserHelper.Operators[j];
                        var spanTemp = spanIterate.Slice(i);

                        if(tempOp.Pattern.Length > spanTemp.Length)
                        {
                            continue;
                        }

                        var isPattern = true;
                        for (int iP = 0; iP < tempOp.Pattern.Length; iP++)
                        {
                            if (spanTemp[iP] != tempOp.Pattern[iP])
                            {
                                isPattern = false;
                                break;
                            }
                        }

                        if(isPattern)
                        {
                            op = tempOp;
                            break;
                        }
                    }

                    if(op != null)
                    {
                        while (
                            stackOperators.TryPeek(out var topOperator) &&
                            topOperator != ParserHelper.LeftBracket &&
                            ((topOperator.Order > op.Order) || (topOperator.Order == op.Order && op.Associativity == Associativity.Left))
                            )
                        {
                            var operatorToOutpit = stackOperators.Pop();
                            var memory = poolMemory.Rent(operatorToOutpit.Pattern.Length);
                            try
                            {
                                var spanMemory = memory.Memory.Span;
                                operatorToOutpit.Pattern.CopyTo(spanMemory);
                                output.Enqueue(new(memory, operatorToOutpit.Pattern.Length, operatorToOutpit.ChunkType));
                            }
                            catch
                            {
                                memory.Dispose();
                                throw;
                            }
                        }
                    }
                }
                catch
                {
                    while (output.TryDequeue(out var cunkExpression))
                        cunkExpression.MemoryOwner.Dispose();

                    throw;
                }
            }
        }
    }
}