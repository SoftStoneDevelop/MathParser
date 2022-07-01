using MathEngine.Enums;
using System.Buffers;

namespace MathEngine.Records
{
    public record ExpressionItem(ChunkType ChunkType);
    public record Operator(string Pattern, ChunkType ChunkType, int Order, Associativity Associativity) : ExpressionItem(ChunkType);

    public record CunkExpression(IMemoryOwner<char> MemoryOwner, int PayloadSize, ExpressionItem Item);
}