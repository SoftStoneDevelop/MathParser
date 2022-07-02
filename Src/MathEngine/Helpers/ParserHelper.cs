using MathEngine.Enums;
using MathEngine.Records;

namespace MathEngine.Helpers
{
    public static class ParserHelper
    {
        public static readonly char[] Numbers = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        #region Operators

        public static readonly Operator[] Operators =
            {
            new Operator("*", ChunkType.Multiplication, 3, Associativity.Left),//0
            new Operator("/", ChunkType.Division, 3, Associativity.Left),//1
            new Operator("+", ChunkType.Addition, 2, Associativity.Left),//2
            new Operator("-", ChunkType.Subtraction, 2, Associativity.Left)//3
        };

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

        #endregion

        #region Functions

        public static readonly Function[] Functions =
            {
            new Function("sin", ChunkType.Sin, 0, Associativity.None, 1)//0
        };

        public static Operator Sin
        {
            get
            {
                return Functions[0];
            }
        }

        #endregion

        //becasue this is special
        public static readonly Operator LeftBracket = new("(", ChunkType.None, 0, Associativity.None);
        public static readonly Operator RightBracket = new(")", ChunkType.None, 0, Associativity.None);
        public static readonly Operand NumberOperand = new(ChunkType.Number);

        public static int IsNumber(ReadOnlySpan<char> chars)
        {
            if (chars.Length < 1)
            {
                throw new ArgumentException("Empty span");
            }

            var spanIterate = chars[..];
            int i = 0;
            if (!Numbers.Contains(spanIterate[i]))
            {
                return -1;
            }

            if(spanIterate.Length > 2 && spanIterate[0] == '0' && spanIterate[1] == '0')
            {
                throw new ArgumentException("Incorrect number");
            }

            bool findSeparator = false;
            for (; i < spanIterate.Length; i++)
            {
                if(spanIterate[i] == ' ')
                {
                    return i;
                }

                if (Numbers.Contains(spanIterate[i]))
                {
                    continue;
                }
                else
                if (chars[i] == ',' || chars[i] == '.')
                {
                    if (findSeparator)
                    {
                        throw new ArgumentException($"Double separator in number '{spanIterate[..(i + 1)]}'");
                    }

                    if (i + 1 == spanIterate.Length)
                    {
                        throw new ArgumentException($"Unexpected end of number '{spanIterate[..(i + 1)]}'");
                    }

                    findSeparator = true;
                    continue;
                }
                else
                {
                    return i;
                }
            }

            return spanIterate.Length;
        }
    }
}