using GymBrain.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace GymBrain.Infrastructure.Services;

public sealed class RedisCacheService(IDistributedCache cache) : ICacheService
{
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        return await cache.GetStringAsync(key, ct);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiry.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiry;

        await cache.SetStringAsync(key, value, options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await cache.RemoveAsync(key, ct);
    }
}
