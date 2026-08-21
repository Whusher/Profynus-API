using System.Text.Json;
using System.Text.Json.Serialization;
using Profynus.Infrastructure.Cache.Interfaces;
using StackExchange.Redis;

namespace Profynus.Infrastructure.Cache.Context;

public class CacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService(IConnectionMultiplexer redis, JsonSerializerOptions? jsonOptions = null)
    {
        _db = redis.GetDatabase();
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        if (expiry != null)
        {
            var ts = expiry.Value;
            await _db.StringSetAsync(key,serialized,ts);
        }
        else
        {// Persistent key
            await _db.StringSetAsync(key, serialized);
        }
        
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await _db.KeyDeleteAsync(key);

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        => await _db.KeyExistsAsync(key);

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null) return cached;

        var value = await factory();
        await SetAsync(key, value, expiry, ct);
        return value;
    }
    
}