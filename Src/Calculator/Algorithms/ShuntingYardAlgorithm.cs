using CalculatorEngine.Helpers;
using CalculatorEngine.Records;
using System.Buffers;

namespace CalculatorEngine.Algorithms
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
                        //TODO
                    }

                    var numberLength = ParserHelper.IsNumber(spanIterate.Slice(i));
                    if (numberLength != -1)
                    {
                        var memory = poolMemory.Rent(numberLength + 1);
                        try
                        {
                            var spanMemory = memory.Memory.Span;
                            spanIterate.Slice(i, numberLength).CopyTo(spanMemory);
                            output.Enqueue(new(memory, numberLength));
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
                        //todo is Operator?
                    }

                    while (
                            stackOperators.TryPeek(out var topOperator) &&
                            topOperator != ParserHelper.LeftBracket &&
                            ((topOperator.Order > op.Order) || (topOperator.Order == op.Order && op.Associativity == Associativity.Left))
                            )
                    {
                        var operatorToOutpit = stackOperators.Pop();
                        var memory = poolMemory.Rent(operatorToOutpit.Name.Length);
                        try
                        {
                            var spanMemory = memory.Memory.Span;
                            operatorToOutpit.Name.CopyTo(spanMemory);
                            output.Enqueue(new(memory, operatorToOutpit.Name.Length));
                        }
                        catch
                        {
                            memory.Dispose();
                            throw;
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