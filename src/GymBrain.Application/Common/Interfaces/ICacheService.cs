namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// Redis cache abstraction.
/// </summary>
public interface ICacheService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task<long> IncrementAsync(string key, TimeSpan? expiry = null, CancellationToken ct = default);
}
