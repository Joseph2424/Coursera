using StackExchange.Redis;
using System.Text.Json;

namespace CacheExpirationsDemo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Connecting to Redis...");

        using var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        IDatabase db = redis.GetDatabase();

        Console.WriteLine("Redis connected.\n");

        await DemonstrateAbsoluteExpiration(db);

        Console.WriteLine("\n---------------------------------\n");

        await DemonstrateSlidingExpiration(db);

        Console.WriteLine("\n---------------------------------\n");

        await DemonstrateDependentExpiration(db);

        Console.WriteLine("\nDemo complete.");
    }

    private static async Task DemonstrateAbsoluteExpiration(IDatabase db)
    {
        Console.WriteLine("ABSOLUTE EXPIRATION");
        Console.WriteLine("-------------------");

        string key = "absolute:user:1";

        var user = new
        {
            Id = 1,
            Name = "John Smith"
        };

        await db.StringSetAsync(
            key,
            JsonSerializer.Serialize(user),
            TimeSpan.FromSeconds(10));

        Console.WriteLine($"Stored '{key}' with 10-second expiration.");

        var value1 = await db.StringGetAsync(key);
        Console.WriteLine($"Immediately: {value1}");

        await Task.Delay(TimeSpan.FromSeconds(12));

        var value2 = await db.StringGetAsync(key);

        Console.WriteLine(
            value2.HasValue
                ? $"Exists: {value2}"
                : "Key expired.");
    }

    private static async Task DemonstrateSlidingExpiration(IDatabase db)
    {
        Console.WriteLine("SLIDING EXPIRATION");
        Console.WriteLine("------------------");

        string key = "sliding:session:1";

        TimeSpan slidingWindow = TimeSpan.FromSeconds(5);

        await db.StringSetAsync(
            key,
            "User Session Data",
            slidingWindow);

        Console.WriteLine($"Stored '{key}' with {slidingWindow.TotalSeconds}s sliding timeout.");

        for (int i = 1; i <= 3; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            var value = await db.StringGetAsync(key);

            if (value.HasValue)
            {
                // Renew expiration after every access
                await db.KeyExpireAsync(key, slidingWindow);

                Console.WriteLine(
                    $"Access #{i}: Key touched and expiration reset.");
            }
        }

        Console.WriteLine("Stopping access...");

        await Task.Delay(TimeSpan.FromSeconds(6));

        var finalValue = await db.StringGetAsync(key);

        Console.WriteLine(
            finalValue.HasValue
                ? "Session still exists."
                : "Session expired due to inactivity.");
    }

    private static async Task DemonstrateDependentExpiration(IDatabase db)
    {
        Console.WriteLine("DEPENDENT EXPIRATION");
        Console.WriteLine("--------------------");

        string parentKey = "customer:100";
        string childKey = "customer:100:orders";

        await db.StringSetAsync(
            parentKey,
            "Customer Data",
            TimeSpan.FromSeconds(15));

        await db.StringSetAsync(
            childKey,
            "Order Data",
            TimeSpan.FromSeconds(30));

        Console.WriteLine("Parent and child cache entries created.");

        var parentExists = await db.KeyExistsAsync(parentKey);
        var childExists = await db.KeyExistsAsync(childKey);

        Console.WriteLine($"Parent Exists: {parentExists}");
        Console.WriteLine($"Child Exists : {childExists}");

        Console.WriteLine("\nDeleting parent key...");

        await db.KeyDeleteAsync(parentKey);

        // Simulated dependency handling
        if (!await db.KeyExistsAsync(parentKey))
        {
            await db.KeyDeleteAsync(childKey);

            Console.WriteLine(
                "Parent removed. Child invalidated automatically.");
        }

        parentExists = await db.KeyExistsAsync(parentKey);
        childExists = await db.KeyExistsAsync(childKey);

        Console.WriteLine($"\nParent Exists: {parentExists}");
        Console.WriteLine($"Child Exists : {childExists}");
    }
}
