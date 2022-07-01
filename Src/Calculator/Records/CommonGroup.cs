using System.Buffers;

namespace CalculatorEngine.Records
{
    public record Operator(string Name, int Order, Associativity Associativity);
    public record CunkExpression(IMemoryOwner<char> MemoryOwner, int PayloadSize);
}