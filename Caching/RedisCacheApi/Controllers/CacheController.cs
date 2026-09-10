using Microsoft.AspNetCore.Mvc;
using RedisCacheApi.Models;
using RedisCacheApi.Services;

namespace RedisCacheApi.Controllers;

[ApiController]
[Route("api/cache")]
public class CacheController(RedisCacheService cacheService) : ControllerBase
{
    private readonly RedisCacheService _cacheService = cacheService;

    [HttpPost]
    public async Task<IActionResult> Create(CacheItem item)
    {
        await _cacheService.SetAsync(item.Key, item.Value, item.ExpirationMinutes);

        return CreatedAtAction(nameof(Get), new { key = item.Key }, item);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var result = await _cacheService.GetAsync(key);

        if (result == null)
            return NotFound();

        return Ok(new { Key = key, Value = result });
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, CacheItem item)
    {
        var existing = await _cacheService.GetAsync(key);

        if (existing == null)
            return NotFound();

        await _cacheService.UpdateAsync(key, item.Value, item.ExpirationMinutes);

        return Ok(new { Message = "Updated" });
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        var removed = await _cacheService.DeleteAsync(key);

        if (!removed)
            return NotFound();

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetKeys()
    {
        var keys = await _cacheService.GetKeysAsync();

        return Ok(keys);
    }
}
