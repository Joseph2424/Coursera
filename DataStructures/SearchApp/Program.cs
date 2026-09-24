using System;
using System.Diagnostics;

namespace SearchApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a sorted array of numbers
            int[] numbers = new int[100000];

            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = i + 1;
            }

            int target = 98765;

            Console.WriteLine($"Searching for: {target}");
            Console.WriteLine();

            // Linear Search Test
            Stopwatch linearTimer = Stopwatch.StartNew();
            int linearComparisons;
            int linearIndex = LinearSearch(numbers, target, out linearComparisons);
            linearTimer.Stop();

            // Binary Search Test
            Stopwatch binaryTimer = Stopwatch.StartNew();
            int binaryComparisons;
            int binaryIndex = BinarySearch(numbers, target, out binaryComparisons);
            binaryTimer.Stop();

            Console.WriteLine("=== Linear Search ===");
            Console.WriteLine($"Index Found: {linearIndex}");
            Console.WriteLine($"Comparisons: {linearComparisons}");
            Console.WriteLine($"Elapsed Time: {linearTimer.ElapsedTicks} ticks");
            Console.WriteLine();

            Console.WriteLine("=== Binary Search ===");
            Console.WriteLine($"Index Found: {binaryIndex}");
            Console.WriteLine($"Comparisons: {binaryComparisons}");
            Console.WriteLine($"Elapsed Time: {binaryTimer.ElapsedTicks} ticks");
            Console.WriteLine();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static int LinearSearch(int[] array, int target, out int comparisons)
        {
            comparisons = 0;

            for (int i = 0; i < array.Length; i++)
            {
                comparisons++;

                if (array[i] == target)
                {
                    return i;
                }
            }

            return -1;
        }

        static int BinarySearch(int[] array, int target, out int comparisons)
        {
            int left = 0;
            int right = array.Length - 1;
            comparisons = 0;

            while (left <= right)
            {
                int middle = left + (right - left) / 2;
                comparisons++;

                if (array[middle] == target)
                {
                    return middle;
                }

                if (array[middle] < target)
                {
                    left = middle + 1;
                }
                else
                {
                    right = middle - 1;
                }
            }

            return -1;
        }
    }
}
