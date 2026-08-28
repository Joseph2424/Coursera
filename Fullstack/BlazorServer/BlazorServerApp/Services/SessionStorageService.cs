using Blazored.SessionStorage;

namespace BlazorServerApp.Services;

public class SessionStorageService(ISessionStorageService session) : ISessionService
{
    private readonly ISessionStorageService _session = session;

    public async Task SetAsync<T>(string key, T value)
    {
        await _session.SetItemAsync(key, value);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await _session.GetItemAsync<T>(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _session.RemoveItemAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var result = await _session.GetItemAsync<object>(key);
        return result is not null;
    }
}
