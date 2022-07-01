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
                    if(spanIterate[i] == ' ')
                    {
                        continue;
                    }

                    if (spanIterate[i] == '(')
                    {
                        stackOperators.Push(ParserHelper.LeftBracket);
                        continue;
                    }

                    if (spanIterate[i] == ')')
                    {
                        if(stackOperators.Count == 0)
                        {
                            throw new InvalidOperationException("The operator stack is empty");
                        }

                        while(
                            stackOperators.TryPeek(out var topOperator) &&
                            topOperator != ParserHelper.LeftBracket
                            )
                        {
                            var operatorToOutpit = stackOperators.Pop();
                            output.Enqueue(new(null, 0, operatorToOutpit));
                        }

                        if(stackOperators.Count == 0)
                        {
                            throw new InvalidOperationException("There are mismatched parentheses");
                        }
                        stackOperators.Pop();
                    }

                    //check is number
                    var numberLength = ParserHelper.IsNumber(spanIterate.Slice(i));
                    if (numberLength != -1)
                    {
                        var memory = poolMemory.Rent(numberLength + 1);
                        try
                        {
                            var spanMemory = memory.Memory.Span;
                            spanIterate.Slice(i, numberLength).CopyTo(spanMemory);
                            output.Enqueue(new(memory, numberLength, ParserHelper.NumberOperand));
                            i += numberLength - 1;
                            continue;
                        }
                        catch
                        {
                            memory.Dispose();
                            throw;
                        }
                    }

                    var spanTemp = spanIterate.Slice(i);
                    //check is function
                    for (int j = 0; j < ParserHelper.Functions.Length; j++)
                    {
                        var tempOp = ParserHelper.Functions[j];
                        if (tempOp.Pattern.Length > spanTemp.Length)
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
                            stackOperators.Push(tempOp);
                    }


                    //check is operator

                    for (int j = 0; j < ParserHelper.Operators.Length; j++)
                    {
                        var tempOp = ParserHelper.Operators[j];
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
                            while (
                                stackOperators.TryPeek(out var topOperator) &&
                                topOperator != ParserHelper.LeftBracket &&
                                ((topOperator.Order > tempOp.Order) || (topOperator.Order == tempOp.Order && tempOp.Associativity == Associativity.Left))
                                )
                            {
                                var operatorToOutpit = stackOperators.Pop();
                                output.Enqueue(new(null, 0, operatorToOutpit));
                            }

                            stackOperators.Push(tempOp);

                            break;
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

            while(stackOperators.TryPop(out var topOperator))
            {
                if(topOperator == ParserHelper.LeftBracket)
                {
                    throw new InvalidOperationException("There are mismatched parentheses");
                }

                output.Enqueue(new(null, 0, topOperator));
            }
        }
    }
}