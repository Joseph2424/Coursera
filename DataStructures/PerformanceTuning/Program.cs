using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        var stopwatch = Stopwatch.StartNew();

        int result = Fibonacci(40);

        stopwatch.Stop();

        Console.WriteLine($"Fibonacci(40) = {result}");
        Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Computes the nth Fibonacci number using an iterative approach.
    /// Uses constant memory and avoids expensive recursive calls.
    /// </summary>
    static int Fibonacci(int n)
    {
        // Base cases
        if (n <= 1)
            return n;

        int prev2 = 0; // F(0)
        int prev1 = 1; // F(1)

        // Build Fibonacci sequence iteratively
        for (int i = 2; i <= n; i++)
        {
            int current = prev1 + prev2;

            // Shift values for next iteration
            prev2 = prev1;
            prev1 = current;
        }

        return prev1;
    }
}