namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// Redis cache abstraction.
/// </summary>
public interface ICacheService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}
