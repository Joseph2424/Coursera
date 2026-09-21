using System;
using System.Diagnostics;

namespace SortingApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int arraySize = 100000;
            const int minValue = 1;
            const int maxValue = 1_000_000;

            Console.WriteLine($"Generating random array of {arraySize:N0} integers...");

            Random random = new();
            int[] originalArray = new int[arraySize];

            for (int i = 0; i < originalArray.Length; i++)
            {
                originalArray[i] = random.Next(minValue, maxValue + 1);
            }

            Console.WriteLine("Array generated.\n");

            RunSort("Bubble Sort", originalArray, BubbleSort);
            RunSort("Merge Sort", originalArray, MergeSort);
            RunSort("Quick Sort", originalArray, QuickSort);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void RunSort(string sortName, int[] originalArray, Action<int[]> sortMethod)
        {
            int[] workingArray = (int[])originalArray.Clone();

            Stopwatch stopwatch = Stopwatch.StartNew();

            sortMethod(workingArray);

            stopwatch.Stop();

            Console.WriteLine($"{sortName,-15} : {stopwatch.ElapsedMilliseconds,6} ms | Sorted: {IsSorted(workingArray)}");
        }

        static bool IsSorted(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < array[i - 1])
                    return false;
            }

            return true;
        }

        #region Bubble Sort

        static void BubbleSort(int[] array)
        {
            int n = array.Length;

            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;

                for (int j = 0; j < n - i - 1; j++)
                {
                    if (array[j] > array[j + 1])
                    {
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        swapped = true;
                    }
                }

                if (!swapped)
                    break;
            }
        }

        #endregion

        #region Merge Sort

        static void MergeSort(int[] array)
        {
            MergeSort(array, 0, array.Length - 1);
        }

        static void MergeSort(int[] array, int left, int right)
        {
            if (left >= right)
                return;

            int middle = (left + right) / 2;

            MergeSort(array, left, middle);
            MergeSort(array, middle + 1, right);

            Merge(array, left, middle, right);
        }

        static void Merge(int[] array, int left, int middle, int right)
        {
            int leftSize = middle - left + 1;
            int rightSize = right - middle;

            int[] leftArray = new int[leftSize];
            int[] rightArray = new int[rightSize];

            Array.Copy(array, left, leftArray, 0, leftSize);
            Array.Copy(array, middle + 1, rightArray, 0, rightSize);

            int i = 0;
            int j = 0;
            int k = left;

            while (i < leftSize && j < rightSize)
            {
                if (leftArray[i] <= rightArray[j])
                {
                    array[k++] = leftArray[i++];
                }
                else
                {
                    array[k++] = rightArray[j++];
                }
            }

            while (i < leftSize)
            {
                array[k++] = leftArray[i++];
            }

            while (j < rightSize)
            {
                array[k++] = rightArray[j++];
            }
        }

        #endregion

        #region Quick Sort

        static void QuickSort(int[] array)
        {
            QuickSort(array, 0, array.Length - 1);
        }

        static void QuickSort(int[] array, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(array, low, high);

                QuickSort(array, low, pivotIndex - 1);
                QuickSort(array, pivotIndex + 1, high);
            }
        }

        static int Partition(int[] array, int low, int high)
        {
            int pivot = array[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (array[j] <= pivot)
                {
                    i++;
                    (array[i], array[j]) = (array[j], array[i]);
                }
            }

            (array[i + 1], array[high]) = (array[high], array[i + 1]);

            return i + 1;
        }

        #endregion
    }
}