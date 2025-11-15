using System;

namespace TestProgram;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Debug test program starting...");
        var x = 10;
        var y = 20;
        var result = Add(x, y);
        Console.WriteLine($"Result: {result}");
        var msg = Transform("Hello");
        Console.WriteLine(msg);
        try { DivideByZero(); }
        catch (Exception ex) { Console.WriteLine($"Caught: {ex.Message}"); }
        Console.WriteLine("Program completed.");
    }

    static int Add(int a, int b)
    {
        var sum = a + b;
        return sum;
    }

    static string Transform(string input)
    {
        var upper = input.ToUpper();
        var result = upper + " World!";
        return result;
    }

    static void DivideByZero()
    {
        var x = 10;
        var y = 0;
        var r = x / y; // throws
    }
}
