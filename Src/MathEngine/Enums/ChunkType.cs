namespace MathEngine.Enums
{
    public enum ChunkType : byte
    {
        //unique types
        None = 0,
        Number = 1,

        //operators
        Multiplication = 10,
        Division = 11,
        Addition = 12,
        Subtraction = 13,

        //functions
        Sin = 100,
    }
}