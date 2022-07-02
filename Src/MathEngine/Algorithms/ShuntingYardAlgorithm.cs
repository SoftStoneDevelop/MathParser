using MathEngine.Enums;
using MathEngine.Helpers;
using MathEngine.Records;
using System.Buffers;
using System.Globalization;
using System.Text;

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
            Queue<ChunkExpression> output
            )
        {
            var spanIterate = chars.Slice(0);
            var stackOperators = new Stack<Operator>();
            int numberLength;

            for (int i = 0; i < spanIterate.Length; i++)
            {
                if (spanIterate[i] == ' ')
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
                    if (stackOperators.Count == 0)
                    {
                        throw new InvalidOperationException("The operator stack is empty");
                    }

                    while (
                        stackOperators.TryPeek(out var topOperator) &&
                        topOperator != ParserHelper.LeftBracket
                        )
                    {
                        var operatorToOutpit = stackOperators.Pop();
                        output.Enqueue(new ChunkExpression(operatorToOutpit));
                    }

                    if (stackOperators.Count == 0)
                    {
                        throw new InvalidOperationException("There are mismatched parentheses 'Left Bracket'");
                    }
                    stackOperators.Pop();
                }

                //check is number
                numberLength = ParserHelper.IsNumber(spanIterate.Slice(i));
                if (numberLength != -1)
                {
                    var numberChars = spanIterate.Slice(i, numberLength);
                    if (!float.TryParse(
                            numberChars,
                            NumberStyles.Any,
                            CultureInfo.CurrentCulture,//TODO replace on correct for ',' and '.' separators
                            out var number)
                            )
                    {
                        throw new ArgumentException($"Invalid number '{numberChars}'");
                    }

                    output.Enqueue(new ChunkNumber(number, ParserHelper.NumberOperand));

                    i += numberLength - 1;

                    continue;
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

                    if (isPattern)
                        stackOperators.Push(tempOp);
                }


                //check is operator

                for (int j = 0; j < ParserHelper.Operators.Length; j++)
                {
                    var tempOp = ParserHelper.Operators[j];
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

                    if (isPattern)
                    {
                        while (
                            stackOperators.TryPeek(out var topOperator) &&
                            topOperator != ParserHelper.LeftBracket &&
                            ((topOperator.Order > tempOp.Order) || (topOperator.Order == tempOp.Order && tempOp.Associativity == Associativity.Left))
                            )
                        {
                            var operatorToOutpit = stackOperators.Pop();
                            output.Enqueue(new ChunkExpression(operatorToOutpit));
                        }

                        stackOperators.Push(tempOp);

                        break;
                    }
                }
            }

            while(stackOperators.TryPop(out var topOperator))
            {
                if(topOperator == ParserHelper.LeftBracket)
                {
                    throw new InvalidOperationException("There are mismatched parentheses 'Left Bracket in end'");
                }

                output.Enqueue(new ChunkExpression(topOperator));
            }
        }

        /// <summary>
        /// Convert infix notation to RPN <see cref="https://en.wikipedia.org/wiki/Shunting_yard_algorithm"/>
        /// </summary>
        /// <param name="chars">Input math expression in infix notation</param>
        /// <param name="output">Output data in the peverse polish notation <see cref="https://en.wikipedia.org/wiki/Reverse_Polish_notation"/></param>
        /// <returns>Real write chars length</returns>
        public static void ToRVNOpt(
            ReadOnlySpan<char> chars,
            Queue<ChunkExpression> output
            )
        {
            var spanIterate = chars.Slice(0);
            var stackOperators = new Stack<Operator>();
            int numberLength;
            var sequenceStack = new Stack<ChunkExpression>();
            
            int sequenceSize = 0;
            int expectedParamsSequnce = 0;

            for (int i = 0; i < spanIterate.Length; i++)
            {
                if (spanIterate[i] == ' ')
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
                    if (stackOperators.Count == 0)
                    {
                        throw new InvalidOperationException("The operator stack is empty");
                    }

                    while (
                        stackOperators.TryPeek(out var topOperator) &&
                        topOperator != ParserHelper.LeftBracket
                        )
                    {
                        var operatorToOutpit = stackOperators.Pop();
                        EnqueueOutput(
                            in sequenceStack,
                            ref sequenceSize,
                            ref expectedParamsSequnce,
                            in output,
                            new ChunkExpression(operatorToOutpit)
                            );
                    }

                    if (stackOperators.Count == 0)
                    {
                        throw new InvalidOperationException("There are mismatched parentheses 'Left Bracket'");
                    }
                    stackOperators.Pop();
                }

                //check is number
                numberLength = ParserHelper.IsNumber(spanIterate.Slice(i));
                if (numberLength != -1)
                {
                    var numberChars = spanIterate.Slice(i, numberLength);
                    if (!float.TryParse(
                            numberChars,
                            NumberStyles.Any,
                            CultureInfo.CurrentCulture,//TODO replace on correct for ',' and '.' separators
                            out var number)
                            )
                    {
                        throw new ArgumentException($"Invalid number '{numberChars}'");
                    }

                    EnqueueOutput(
                        in sequenceStack,
                        ref sequenceSize,
                        ref expectedParamsSequnce,
                        in output,
                        new ChunkNumber(number, ParserHelper.NumberOperand)
                        );
                    i += numberLength - 1;

                    continue;
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

                    if (isPattern)
                        stackOperators.Push(tempOp);
                }


                //check is operator
                for (int j = 0; j < ParserHelper.Operators.Length; j++)
                {
                    var tempOp = ParserHelper.Operators[j];
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

                    if (isPattern)
                    {
                        while (
                            stackOperators.TryPeek(out var topOperator) &&
                            topOperator != ParserHelper.LeftBracket &&
                            ((topOperator.Order > tempOp.Order) || (topOperator.Order == tempOp.Order && tempOp.Associativity == Associativity.Left))
                            )
                        {
                            var operatorToOutpit = stackOperators.Pop();
                            EnqueueOutput(
                                in sequenceStack,
                                ref sequenceSize,
                                ref expectedParamsSequnce,
                                in output,
                                new ChunkExpression(operatorToOutpit)
                                );
                        }

                        stackOperators.Push(tempOp);

                        break;
                    }
                }
            }

            while (stackOperators.TryPop(out var topOperator))
            {
                if (topOperator == ParserHelper.LeftBracket)
                {
                    throw new InvalidOperationException("There are mismatched parentheses 'Left Bracket in end'");
                }

                EnqueueOutput(
                    in sequenceStack,
                    ref sequenceSize,
                    ref expectedParamsSequnce,
                    in output,
                    new ChunkExpression(topOperator)
                    );
            }

            if(sequenceStack.TryPop(out var shoulBeOperator))
            {
                if(shoulBeOperator is ChunkNumber)
                {
                    throw new Exception("Sequence mustd ended on number");
                }

                if(sequenceStack.Count == 0)
                {
                    output.Enqueue(shoulBeOperator);
                }
                else
                {
                    WriteSequence(
                        in sequenceStack,
                        ref sequenceSize,
                        ref expectedParamsSequnce,
                        in output,
                        shoulBeOperator.Item
                        );
                }
            }
        }

        /// <summary>
        /// Enqueue to Output queue and apply optimization
        /// </summary>
        private static void EnqueueOutput(
            in Stack<ChunkExpression> sequenceStack,
            ref int sequenceSize,
            ref int expectedParamsCount,
            in Queue<ChunkExpression> output,
            in ChunkExpression newChunk
            )
        {
            switch(newChunk.Item.ChunkType)
            {
                case ChunkType.Number:
                {
                    if(!sequenceStack.TryPeek(out var chunk0Peek))
                    {
                        sequenceStack.Push(newChunk);
                        sequenceSize++;
                        expectedParamsCount++;
                        return;
                    }

                    if(chunk0Peek is ChunkNumber)
                    {
                        sequenceStack.Pop();
                        sequenceSize--;
                        expectedParamsCount--;

                        if (sequenceStack.Count != 0)
                        {
                            var chunk1 = sequenceStack.Pop();
                            if (chunk1 is ChunkNumber)
                            {
                                throw new Exception("Incorrect sequence");
                            }

                            WriteSequence(in sequenceStack, ref sequenceSize, ref expectedParamsCount, in output, chunk1.Item);
                        }

                        output.Enqueue(chunk0Peek);
                        sequenceStack.Push(newChunk);
                        sequenceSize++;
                        expectedParamsCount++;

                        return;
                    }
                    else
                    {
                        sequenceStack.Push(newChunk);
                        sequenceSize++;
                        expectedParamsCount++;
                        return;
                    }
                }

                case ChunkType.Multiplication:
                case ChunkType.Addition:
                case ChunkType.Subtraction:
                {
                    if (!sequenceStack.TryPop(out var chunk0))
                    {
                        sequenceStack.Push(newChunk);
                        return;
                    }
                    else
                    {
                        if(chunk0 is ChunkNumber)
                        {
                            if(sequenceStack.Count == 0)
                            {
                                sequenceStack.Push(chunk0);
                                sequenceStack.Push(newChunk);
                                return;
                            }
                            else
                            {
                                var chunk1 = sequenceStack.Pop();
                                if (chunk1 is ChunkNumber)
                                {
                                    throw new Exception("Incorrect sequence");
                                }

                                if (chunk1.Item.ChunkType == newChunk.Item.ChunkType)
                                {
                                    sequenceStack.Push(chunk0);
                                    sequenceStack.Push(newChunk);
                                    return;
                                }
                                else
                                {
                                    WriteSequence(in sequenceStack, ref sequenceSize, ref expectedParamsCount, in output, chunk1.Item);

                                    sequenceStack.Push(chunk0);
                                    sequenceSize = 1;
                                    expectedParamsCount = 1;
                                    sequenceStack.Push(newChunk);
                                }
                            }
                        }
                        else
                        {
                            if (chunk0.Item.ChunkType == newChunk.Item.ChunkType)
                            {
                                sequenceStack.Push(chunk0);
                                expectedParamsCount++;
                            }
                            else
                            {
                                WriteSequence(in sequenceStack, ref sequenceSize, ref expectedParamsCount, in output, chunk0.Item);
                                output.Enqueue(newChunk);
                            }
                        }
                    }

                    break;
                }

                default:
                {
                    if(sequenceStack.Count == 0)
                    {
                        output.Enqueue(newChunk);
                        return;
                    }
                    else
                    {
                        var chunk0 = sequenceStack.Pop();
                        if (chunk0 is ChunkNumber)
                        {
                            sequenceSize--;
                            expectedParamsCount--;

                            if(sequenceStack.TryPop(out var chunk1))
                            {
                                if (chunk1 is ChunkNumber)
                                {
                                    throw new Exception("Incorrect sequence");
                                }

                                WriteSequence(in sequenceStack, ref sequenceSize, ref expectedParamsCount, in output, chunk1.Item);
                            }
                            
                            output.Enqueue(chunk0);
                        }
                        else
                        {
                            WriteSequence(in sequenceStack, ref sequenceSize, ref expectedParamsCount, in output, chunk0.Item);
                        }
                        
                        output.Enqueue(newChunk);
                    }

                    break;
                }

            }
        }

        private static void WriteSequence(
            in Stack<ChunkExpression> sequenceStack,
            ref int sequenceSize,
            ref int expectedParamsCount,
            in Queue<ChunkExpression> output,
            in ExpressionItem @operator
            )
        {
            var memory = MemoryPool<float>.Shared.Rent(expectedParamsCount + 1);
            try
            {
                var spanMemory = memory.Memory.Span[(expectedParamsCount + 1 - sequenceSize)..];
                int indexSpan = 0;
                while(sequenceStack.TryPop(out var item))
                {
                    if(item is ChunkNumber chunkNumber)
                    {
                        spanMemory[indexSpan] = chunkNumber.Number;
                        indexSpan++;
                    }
                }

                output.Enqueue(new SequenceNumberOperation(memory, sequenceSize, expectedParamsCount + 1, (PatternExpression)@operator));
            }
            catch
            {
                memory.Dispose();
                throw;
            }
            finally
            {
                sequenceSize = 0;
                expectedParamsCount = 0;
            }
        }

        public static void PrintRVNAndDequeueAll(Queue<ChunkExpression> rvn, StringBuilder builder)
        {
            while(rvn.TryDequeue(out var item))
            {
                switch(item)
                {
                    case ChunkNumber chunkNumber:
                    {
                        builder.Append(chunkNumber.Number);
                        builder.Append(' ');

                        break;
                    }

                    case SequenceNumberOperation sequenceNumberOperation:
                    {
                        try
                        {
                            var startIndex = sequenceNumberOperation.ExpectedParamsCount - sequenceNumberOperation.Size;
                            var spanTemp = 
                                sequenceNumberOperation.SequenceMemory.Memory.Span
                                .Slice(0, sequenceNumberOperation.ExpectedParamsCount);

                            for (; startIndex < spanTemp.Length; startIndex++)
                            {
                                builder.Append(spanTemp[startIndex]);
                                builder.Append(' ');
                            }

                            builder.Append($"S{sequenceNumberOperation.PatternItem.Pattern}({sequenceNumberOperation.ExpectedParamsCount}) ");
                        }
                        finally
                        {
                            sequenceNumberOperation.SequenceMemory.Dispose();
                        }
                        
                        break;
                    }

                    case ChunkExpression chunkExpression:
                    {
                        var patternExp = (PatternExpression)chunkExpression.Item;
                        builder.Append(patternExp.Pattern);
                        builder.Append(' ');

                        break;
                    }
                }
            }
        }
    }
}