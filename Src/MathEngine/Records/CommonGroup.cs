using MathEngine.Enums;
using System.Buffers;

namespace MathEngine.Records
{
    public record ExpressionItem(ChunkType ChunkType);
    public record Operand(ChunkType ChunkType) : ExpressionItem(ChunkType);
    public record PatternExpression(string Pattern, ChunkType ChunkType) : ExpressionItem(ChunkType);
    public record Operator(string Pattern, ChunkType ChunkType, int Order, Associativity Associativity) : PatternExpression(Pattern, ChunkType);
    public record Function(string Pattern, ChunkType ChunkType, int Order, Associativity Associativity, int ParametrsCount) 
        : Operator(Pattern, ChunkType, Order, Associativity);

    public record ChunkExpression(ExpressionItem Item);
    public record ChunkNumber(float Number, Operand OperandItem) : ChunkExpression(OperandItem);
    public record SequenceNumberOperation(
        IMemoryOwner<float> SequenceMemory,
        int Size,
        int ExpectedParamsCount,
        PatternExpression PatternItem
        ) : ChunkExpression(PatternItem);
}