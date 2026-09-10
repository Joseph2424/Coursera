using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMemoryCache(options =>
{
    // Total cache size limit
    options.SizeLimit = 1024;
});

var serviceProvider = services.BuildServiceProvider();

var cache = serviceProvider.GetRequiredService<IMemoryCache>();

const string ProductsCacheKey = "products";

Console.WriteLine("First Request");
DisplayProducts(cache);

Console.WriteLine("\nSecond Request");
DisplayProducts(cache);

Console.WriteLine("\nWaiting 15 seconds for cache expiration...");
await Task.Delay(TimeSpan.FromSeconds(15));

Console.WriteLine("\nThird Request");
DisplayProducts(cache);

Console.WriteLine("Removing 'ProductList' from cache...");
cache.Remove("ProductList");

if (!cache.TryGetValue("ProductList", out _))
{
    Console.WriteLine("Cache entry 'ProductList' successfully removed.");
}
else
{
    Console.WriteLine("Cache entry 'ProductList' still exists.");
}

static void DisplayProducts(IMemoryCache cache)
{
    if (cache.TryGetValue(ProductsCacheKey, out List<Product>? products))
    {
        Console.WriteLine("Products loaded from cache.");
    }
    else
    {
        Console.WriteLine("Cache miss. Fetching products...");

        products = FetchProducts();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
            .SetSize(1); // Required when SizeLimit is used

        cache.Set(ProductsCacheKey, products, cacheOptions);
    }

    foreach (var product in products!)
    {
        Console.WriteLine($"Id: {product.Id}, Name: {product.Name}, Price: {product.Price:C}");
    }
}

static List<Product> FetchProducts()
{
    Console.WriteLine("Simulating database/API call...");

    return new List<Product>
    {
        new Product(1, "Laptop", 999.99m),
        new Product(2, "Mouse", 29.99m),
        new Product(3, "Keyboard", 79.99m),
        new Product(4, "Monitor", 249.99m),
    };
}

public record Product(int Id, string Name, decimal Price);
