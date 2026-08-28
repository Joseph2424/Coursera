using Microsoft.Extensions.Caching.Memory;

namespace BlazorServerApp.Services;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private readonly IMemoryCache _cache = cache;

    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T value) ? value : default;
    }

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public bool Exists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }
}
