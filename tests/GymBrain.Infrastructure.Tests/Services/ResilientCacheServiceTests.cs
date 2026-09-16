using GymBrain.Application.Common.Exceptions;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Infrastructure;
using GymBrain.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;

namespace GymBrain.Infrastructure.Tests.Services;

public class ResilientCacheServiceTests
{
    [Fact]
    public async Task MissingRedis_AllowsCacheOperations_ButRefusesUsageCounters()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Jwt:Secret"] = new string('x', 32) }).Build());
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        Assert.Null(await cache.GetAsync("missing"));
        await cache.SetAsync("key", "value");
        await cache.RemoveAsync("key");
        await Assert.ThrowsAsync<CacheUnavailableException>(() => cache.IncrementAsync("limit"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RedisOutage_DegradesPayloadCache_ButRefusesUsageCounters(bool timeout)
    {
        Exception failure = timeout
            ? new RedisTimeoutException("test timeout", CommandStatus.Unknown)
            : new RedisConnectionException(ConnectionFailureType.UnableToConnect, "test outage");
        var cache = new ResilientCacheService(new FailingCache(failure), NullLogger<ResilientCacheService>.Instance);
        Assert.Null(await cache.GetAsync("key"));
        await cache.SetAsync("key", "value");
        await cache.RemoveAsync("key");
        var error = await Assert.ThrowsAsync<CacheUnavailableException>(() => cache.IncrementAsync("limit"));
        Assert.Same(failure, error.InnerException);
    }

    private sealed class FailingCache(Exception failure) : ICacheService
    {
        public Task<string?> GetAsync(string key, CancellationToken ct = default) => throw failure;
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class => throw failure;
        public Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default) => throw failure;
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class => throw failure;
        public Task RemoveAsync(string key, CancellationToken ct = default) => throw failure;
        public Task<long> IncrementAsync(string key, TimeSpan? expiry = null, CancellationToken ct = default) => throw failure;
    }
}
