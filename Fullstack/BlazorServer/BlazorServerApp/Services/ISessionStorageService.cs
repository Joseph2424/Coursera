namespace BlazorServerApp.Services;

public interface ISessionService
{
    Task SetAsync<T>(string key, T value);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}
