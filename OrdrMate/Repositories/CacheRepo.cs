using System.Text.Json;
using StackExchange.Redis;

namespace OrdrMate.Repositories;

public class CacheRepo<T>(IConnectionMultiplexer redis)
{
    protected readonly IDatabase _cache = redis.GetDatabase();
    private static bool _needUpdate = true;

    public async Task<T?> GetAsync(string key)
    {
        var value = await _cache.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync(string key, T value, TimeSpan expiry = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _cache.StringSetAsync(key, json, expiry);
        _needUpdate = false;
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.KeyDeleteAsync(key);
    }

    public void MarkForUpdate()
    {
        _needUpdate = true;
    }

    public bool NeedsUpdate()
    {
        return _needUpdate;
    }
}