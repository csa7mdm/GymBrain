using System.Text.Json;
using GymBrain.Application.Common.Exceptions;
using GymBrain.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GymBrain.Infrastructure.Services;

// Cache payloads are optional; usage counters must fail closed to preserve AI limits.
public sealed class ResilientCacheService(ICacheService? inner, ILogger<ResilientCacheService> logger) : ICacheService
{
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        try { return inner is null ? null : await inner.GetAsync(key, ct); }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            logger.LogWarning("Redis unavailable; treating cache read as a miss.");
            return null;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var value = await GetAsync(key, ct);
        return value is null ? null : JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try { if (inner is not null) await inner.SetAsync(key, value, expiry, ct); }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            logger.LogWarning("Redis unavailable; skipping cache write.");
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
        => SetAsync(key, JsonSerializer.Serialize(value), expiry, ct);

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { if (inner is not null) await inner.RemoveAsync(key, ct); }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            logger.LogWarning("Redis unavailable; skipping cache removal.");
        }
    }

    public async Task<long> IncrementAsync(string key, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        if (inner is null) throw new CacheUnavailableException();
        try { return await inner.IncrementAsync(key, expiry, ct); }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            logger.LogWarning("Redis unavailable; refusing operation requiring usage limits.");
            throw new CacheUnavailableException(ex);
        }
    }

    private static bool IsUnavailable(Exception ex) => ex is RedisConnectionException or RedisTimeoutException;
}
