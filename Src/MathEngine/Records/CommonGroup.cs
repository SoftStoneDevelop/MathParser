using MathEngine.Enums;

namespace MathEngine.Records
{
    public record ExpressionItem(ChunkType ChunkType);
    public record Operand(ChunkType ChunkType) : ExpressionItem(ChunkType);
    public record Operator(string Pattern, ChunkType ChunkType, int Order, Associativity Associativity) : ExpressionItem(ChunkType);
    public record Function(string Pattern, ChunkType ChunkType, int Order, Associativity Associativity, int ParametrsCount) 
        : Operator(Pattern, ChunkType, Order, Associativity);

    public record ChunkExpression(ExpressionItem Item);
    public record ChunkNumber(float Number, ExpressionItem Item) : ChunkExpression(Item);
}