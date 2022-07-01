namespace MathEngine
{
    public class Calculator : ICalculator
    {
        public float CalculateExpression(string expression)
        {
            //((3 * 5) + 2 - 11) / 3
            //3 * 5 + 2 - (11 / 3)

            var charsSpan = expression.AsSpan();
            charsSpan = charsSpan.Slice(0);
            return 0f;
        }
    }
}