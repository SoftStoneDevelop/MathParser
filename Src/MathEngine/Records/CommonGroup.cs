using MathEngine.Enums;
using System.Buffers;

namespace MathEngine.Records
{
    public record ExpressionItem(ChunkType ChunkType);
    public record Operand(ChunkType ChunkType) : ExpressionItem(ChunkType);
    public record Operator(string Pattern, ChunkType ChunkType, int Order, Associativity Associativity) : ExpressionItem(ChunkType);
    public record Function(string Pattern, ChunkType ChunkType, int Order, Associativity Associativity, int ParametrsCount) 
        : Operator(Pattern, ChunkType, Order, Associativity);

    public record ChunkExpression(IMemoryOwner<char> MemoryOwner, int PayloadSize, ExpressionItem Item);
}