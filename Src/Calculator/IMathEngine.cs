namespace MathEngine
{
    public interface IMathEngine
    {
        float CalculateExpression(string expression);

        float CalculateExpression(ReadOnlySpan<char> expression);
    }
}