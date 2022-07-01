using MathEngine.Enums;
using System.Buffers;

namespace MathEngine.Records
{
    public record Operator(string Pattern, int Order, Associativity Associativity, ChunkType ChunkType);
    public record CunkExpression(IMemoryOwner<char> MemoryOwner, int PayloadSize, ChunkType chunkType);
}