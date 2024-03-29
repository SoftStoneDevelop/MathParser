﻿using System;

namespace ConsoleCalculator
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("Write 'Exit' for close application");
            var engine = new MathEngine.MathEngine();
            Console.ForegroundColor = ConsoleColor.White;

            while (true)
            {
                var expression = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(expression))
                {
                    continue;
                }

                if(expression == "Exit")
                {
                    break;
                }

                try
                {
                    var result = engine.CalculateExpression(expression);
                    Console.WriteLine($"Result:= {result}");
                }
                catch(Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }   
    }
}