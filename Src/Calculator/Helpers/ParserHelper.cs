using MathEngine.Enums;
using MathEngine.Records;

namespace MathEngine.Helpers
{
    public static class ParserHelper
    {
        public static readonly char[] Numbers = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        public static readonly Operator[] Operators =
            {
            new Operator("*", 3, Associativity.Left, ChunkType.Multiplication),
            new Operator("/", 3, Associativity.Left, ChunkType.Division),
            new Operator("+", 2, Associativity.Left, ChunkType.Addition),
            new Operator("-", 2, Associativity.Left, ChunkType.Subtraction)
        };

        //becasue this two operators is special
        public static readonly Operator LeftBracket = new("(", 0, Associativity.None, ChunkType.None);
        public static readonly Operator RightBracket = new(")", 0, Associativity.None, ChunkType.None);

        public static Operator Multiplication
        {
            get
            {
                return Operators[0];
            }
        }

        public static Operator Division
        {
            get
            {
                return Operators[1];
            }
        }

        public static Operator Addition
        {
            get
            {
                return Operators[2];
            }
        }

        public static Operator Subtraction
        {
            get
            {
                return Operators[3];
            }
        }

        public static int IsNumber(ReadOnlySpan<char> chars)
        {
            if (chars.Length < 1)
            {
                throw new ArgumentException("Empty span");
            }

            var spanIterate = chars.Slice(0);
            int i = 0;
            if (!Numbers.Contains(spanIterate[i]))
            {
                return -1;
            }

            if (spanIterate.Length > 1 && spanIterate[i] == '0' && spanIterate[i + 1] != '.' && spanIterate[i + 1] != ',')
            {
                throw new ArgumentException("Incorrect number");
            }

            bool findSeparator = false;
            for (; i < spanIterate.Length; i++)
            {
                if (Numbers.Contains(spanIterate[i]))
                {
                    continue;
                }

                if (chars[i] == ',' || chars[i] == '.')
                {
                    if (findSeparator)
                    {
                        throw new ArgumentException($"Double separator in number '{spanIterate.Slice(0, i + 1)}'");
                    }

                    if (i + 1 == spanIterate.Length)
                    {
                        throw new ArgumentException($"Unexpected end of number '{spanIterate.Slice(0, i + 1)}'");
                    }

                    findSeparator = true;
                    continue;
                }

                return i;
            }

            return spanIterate.Length;
        }
    }
}