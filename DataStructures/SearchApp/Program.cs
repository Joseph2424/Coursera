using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SearchApp
{
    class Program
    {
        static async Task Main()
        {
            // Create a sorted array of numbers
            // int[] numbers = new int[100000];

            // for (int i = 0; i < numbers.Length; i++)
            // {
            //     numbers[i] = i + 1;
            // }

            // int target = 98765;

            //Console.WriteLine($"Searching for: {target}");
            //Console.WriteLine();

            Console.WriteLine("Retrieving users from API...");

            using HttpClient client = new();

            var response = await client.GetFromJsonAsync<RandomUserResponse>(
                "https://randomuser.me/api/?results=1000&inc=name&nat=us"
            );

            if (response?.Results == null)
            {
                Console.WriteLine("Failed to retrieve users.");
                return;
            }

            // Build searchable dataset
            string[] names = [.. response
                .Results.Select(u => $"{u.Name.First} {u.Name.Last}")];

            Console.WriteLine("\nUsers Retrieved:");
            foreach (var name in names)
            {
                Console.WriteLine($" - {name}");
            }

            // Pick a user to search for
            string target = names[5];

            Console.WriteLine($"\nSearching for: \"{target}\"");

            // Linear Search Test
            Stopwatch linearTimer = Stopwatch.StartNew();
            int linearIndex = LinearSearch(names, target, out int linearComparisons);
            linearTimer.Stop();

            // Binary Search requires sorted data
            string[] sortedNames = [.. names.OrderBy(n => n)];

            // Binary Search Test
            Stopwatch binaryTimer = Stopwatch.StartNew();
            int binaryIndex = BinarySearch(sortedNames, target, out int binaryComparisons);
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

        static int LinearSearch(string[] array, string target, out int comparisons)
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

        static int BinarySearch(string[] array, string target, out int comparisons)
        {
            int left = 0;
            int right = array.Length - 1;
            comparisons = 0;

            while (left <= right)
            {
                comparisons++;

                int mid = left + (right - left) / 2;

                int result = string.Compare(array[mid], target, StringComparison.OrdinalIgnoreCase);

                if (result == 0)
                    return mid;

                if (result < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }
    }

    public class RandomUserResponse
    {
        [JsonPropertyName("results")]
        public List<User> Results { get; set; } = [];
    }

    public class User
    {
        [JsonPropertyName("name")]
        public Name Name { get; set; } = new();
    }

    public class Name
    {
        [JsonPropertyName("first")]
        public string First { get; set; } = string.Empty;

        [JsonPropertyName("last")]
        public string Last { get; set; } = string.Empty;
    }
}
