using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace RedisCacheApi.Services;

public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis)
{
    private readonly IDistributedCache _cache = cache;
    private readonly IConnectionMultiplexer _redis = redis;

    public async Task SetAsync(string key, string value, int expirationMinutes)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
        };

        await _cache.SetStringAsync(key, value, options);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _cache.GetStringAsync(key);
    }

    public async Task UpdateAsync(string key, string value, int expirationMinutes)
    {
        await SetAsync(key, value, expirationMinutes);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var existing = await GetAsync(key);

        if (existing is null)
            return false;

        await _cache.RemoveAsync(key);

        return true;
    }

    public async Task<IEnumerable<string>> GetKeysAsync()
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        var keys = server.Keys();

        return await Task.FromResult(keys.Select(k => k.ToString()));
    }
}
