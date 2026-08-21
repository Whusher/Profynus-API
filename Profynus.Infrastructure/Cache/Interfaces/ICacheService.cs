namespace Profynus.Infrastructure.Cache.Interfaces;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key, CancellationToken token = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration, CancellationToken token = default);
    Task RemoveAsync(string key, CancellationToken token = default);
    Task<bool> ExistsAsync(string key, CancellationToken token = default);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default);
}