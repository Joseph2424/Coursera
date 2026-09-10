namespace RedisCacheApi.Models;

public class CacheItem
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}
