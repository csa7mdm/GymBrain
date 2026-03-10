using System.Text.Json;
using GymBrain.Application.Common.Interfaces;
using StackExchange.Redis;

namespace GymBrain.Infrastructure.Services;

public sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private const string LegacyDataField = "data";
    private const string Prefix = "GymBrain:";
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var fullKey = BuildKey(key);

        try
        {
            var value = await _db.StringGetAsync(fullKey);
            if (!value.IsNull)
                return value.ToString();
        }
        catch (RedisServerException ex) when (IsWrongType(ex))
        {
            return await TryGetLegacyHashValueAsync(fullKey);
        }

        return await TryGetLegacyHashValueAsync(fullKey);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var value = await GetAsync(key, ct);
        return value == null ? null : JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var fullKey = BuildKey(key);
        await _db.KeyDeleteAsync(fullKey);
        await _db.StringSetAsync(fullKey, value, expiry);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await SetAsync(key, json, expiry, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(BuildKey(key));
    }

    public async Task<long> IncrementAsync(string key, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var fullKey = BuildKey(key);

        try
        {
            var newValue = await _db.StringIncrementAsync(fullKey);
            if (newValue == 1 && expiry.HasValue)
                await _db.KeyExpireAsync(fullKey, expiry.Value);

            return newValue;
        }
        catch (RedisServerException ex) when (IsWrongType(ex))
        {
            await _db.KeyDeleteAsync(fullKey);
            var newValue = await _db.StringIncrementAsync(fullKey);
            if (expiry.HasValue)
                await _db.KeyExpireAsync(fullKey, expiry.Value);

            return newValue;
        }
    }

    private static bool IsWrongType(RedisServerException ex) =>
        ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase);

    private static RedisKey BuildKey(string key) => $"{Prefix}{key}";

    private async Task<string?> TryGetLegacyHashValueAsync(RedisKey fullKey)
    {
        var value = await _db.HashGetAsync(fullKey, LegacyDataField);
        return value.IsNull ? null : value.ToString();
    }
}
