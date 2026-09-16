using GymBrain.Application.Common.Interfaces;
using GymBrain.Application.Orchestration.Commands;
using GymBrain.Application.Orchestration.Queries;
using GymBrain.Domain.Entities;
using GymBrain.Domain.Enums;
using GymBrain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymBrain.Infrastructure.Tests.Persistence;

public sealed class WorkoutCompletionTests : IAsyncLifetime
{
    private readonly string path = Path.Combine(Path.GetTempPath(), $"gymbrain-test-{Guid.NewGuid()}.db");
    private Guid userId;
    private Guid otherId;
    private const string Payload = """{"schemaVersion":1,"focus":"Strength","exercises":[{"name":"Squat","sets":[{"completed":true,"reps":8,"weightKg":25}]}]}""";

    private GymBrainDbContext Open(params IInterceptor[] interceptors) => new(new DbContextOptionsBuilder<GymBrainDbContext>()
        .UseSqlite($"Data Source={path};Pooling=False").AddInterceptors(interceptors).Options);
    public async Task InitializeAsync()
    {
        await using var db = Open();
        await db.Database.EnsureCreatedAsync();
        var user = new User("one@example.invalid", "hash", ExperienceLevel.Beginner);
        var other = new User("two@example.invalid", "hash", ExperienceLevel.Beginner);
        db.Users.AddRange(user, other);
        await db.SaveChangesAsync();
        userId = user.Id; otherId = other.Id;
    }
    public Task DisposeAsync() { File.Delete(path); return Task.CompletedTask; }
    private static SaveWorkoutCommandHandler Handler(GymBrainDbContext db) => new(db, new NoMilestones());

    [Fact]
    public async Task RetryAcrossContextsKeepsOneImmutableSessionAndOneIncrement()
    {
        var id = Guid.NewGuid();
        await using (var db = Open()) await Handler(db).Handle(new(userId, Payload, id), default);
        await using (var db = Open()) await Handler(db).Handle(new(userId, Payload.Replace("25", "50"), id), default);
        await using var verify = Open();
        var session = await verify.WorkoutSessions.SingleAsync();
        Assert.Equal(Payload, session.PayloadJson);
        Assert.Equal(1, (await verify.Users.SingleAsync(u => u.Id == userId)).WorkoutsCompleted);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CompetingSavesAreAtomicAndDoNotLoseCounts(bool sameSession)
    {
        var id = Guid.NewGuid();
        var competingId = sameSession ? id : Guid.NewGuid();
        // Commit another request after this handler reads the counter, before its save.
        var race = new BeforeFirstSave(async () => {
            await using var competing = Open();
            await Handler(competing).Handle(new(userId, Payload, competingId), default);
        });
        await using (var db = Open(race)) await Handler(db).Handle(new(userId, Payload, id), default);
        await using var verify = Open();
        Assert.Equal(sameSession ? 1 : 2, await verify.WorkoutSessions.CountAsync());
        Assert.Equal(sameSession ? 1 : 2, (await verify.Users.SingleAsync(u => u.Id == userId)).WorkoutsCompleted);
    }

    [Fact]
    public async Task SessionIdCannotBeReusedByAnotherAccount()
    {
        var id = Guid.NewGuid();
        await using (var db = Open()) await Handler(db).Handle(new(userId, Payload, id), default);
        await using var other = Open();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Handler(other).Handle(new(otherId, Payload, id), default));
        Assert.Equal(0, (await other.Users.SingleAsync(u => u.Id == otherId)).WorkoutsCompleted);
    }

    [Fact]
    public async Task HistoryIsScopedOrderedAndPaginated()
    {
        await using var db = Open();
        for (var i = 0; i < 52; i++) {
            var session = new WorkoutSession(userId, Payload) { CreatedAtUtc = DateTime.UtcNow.AddMinutes(-i) };
            session.MarkCompleted(); db.WorkoutSessions.Add(session);
        }
        var hidden = new WorkoutSession(otherId, Payload); hidden.MarkCompleted(); db.WorkoutSessions.Add(hidden);
        db.WorkoutSessions.Add(new WorkoutSession(userId, Payload)); // unfinished
        await db.SaveChangesAsync();
        var handler = new GetWorkoutHistoryQueryHandler(db);
        var first = await handler.Handle(new(userId), default);
        var second = await handler.Handle(new(userId, 50), default);
        Assert.Equal(52, first.Total); Assert.Equal(50, first.Items.Count); Assert.True(first.HasMore);
        Assert.Equal(2, second.Items.Count); Assert.False(second.HasMore);
        Assert.True(first.Items.Last().CompletedAtUtc > second.Items.First().CompletedAtUtc);
        Assert.DoesNotContain(first.Items.Concat(second.Items), item => item.Id == hidden.Id);
    }

    [Theory]
    [InlineData("{", false)]
    [InlineData("{}", false)]
    [InlineData("[]", false)]
    [InlineData(Payload, true)]
    [InlineData("{\"schemaVersion\":1,\"focus\":\"test\",\"exercises\":[]}", false)]
    public void CompletionValidationRejectsMalformedOrEmptyResults(string json, bool valid)
    {
        Assert.Equal(valid, new SaveWorkoutCommandValidator().Validate(new SaveWorkoutCommand(userId, json, Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void ProductionModelMatchesMigrationSnapshot()
    {
        using var db = new GymBrainDbContext(new DbContextOptionsBuilder<GymBrainDbContext>()
            .UseNpgsql("Host=localhost;Database=model_check;Username=unused;Password=unused").Options);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task GenerationUsesCurrentProfileAndCacheVariesWithEveryRestriction()
    {
        await using var db = Open();
        var user = await db.Users.SingleAsync(u => u.Id == userId);
        user.VaultApiKey("test-key", "openai");
        user.UpdateProfile("strength", "[]", "", 3, "Standard", 2000, ExperienceLevel.Beginner);
        await db.SaveChangesAsync();
        var provider = new FakeProvider(); var cache = new MemoryCache(); var limits = new CountingLimiter();
        var handler = new StartWorkoutCommandHandler(db, new TestVault(), provider, cache, limits,
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        var command = new StartWorkoutCommand(userId, ExperienceLevel.Advanced, "core");
        var first = await handler.Handle(command, default);
        Assert.Contains("Level: beginner", provider.LastMessage);
        Assert.Contains("40", first.MegaPayloadJson); // current profile, not stale token level
        Assert.DoesNotContain("Barbell Squat", provider.LastPrompt); // unavailable equipment
        await handler.Handle(command, default); Assert.Equal(1, provider.Calls);
        foreach (var change in new[] { "level", "equipment", "injuries", "goal", "focus" })
        {
            user.UpdateProfile(change == "goal" ? "health" : user.Goal,
                change == "equipment" ? "[\"Dumbbells\"]" : user.EquipmentJson,
                change == "injuries" ? "knee pain" : user.Injuries, 3, "Standard", 2000,
                change == "level" ? ExperienceLevel.Advanced : user.ExperienceLevel);
            await db.SaveChangesAsync();
            if (change == "focus") command = command with { WorkoutFocus = "upper body" };
            await handler.Handle(command, default);
        }
        Assert.Equal(6, provider.Calls); Assert.Equal(7, limits.Calls);
        Assert.DoesNotContain("Goblet Squat", provider.LastPrompt);
    }

    [Fact]
    public async Task SubstitutionReadsCurrentRestrictionsAndEquipment()
    {
        await using var db = Open();
        var user = await db.Users.SingleAsync(u => u.Id == userId);
        user.UpdateProfile("strength", "[\"Dumbbells\"]", "knee pain", 3, "Standard", 2000, ExperienceLevel.Beginner);
        await db.SaveChangesAsync();
        var limiter = new CountingLimiter();
        var handler = new GetSubstituteQueryHandler(db, limiter);
        var query = new GetSubstituteQuery(userId, Guid.Parse("10000001-0000-0000-0000-000000000001"), "");
        Assert.Empty((await handler.Handle(query, default)).Substitutes);
        user.UpdateProfile("strength", "[\"Dumbbells\"]", "", 3, "Standard", 2000, ExperienceLevel.Beginner);
        await db.SaveChangesAsync();
        Assert.Equal("Goblet Squat", Assert.Single((await handler.Handle(query, default)).Substitutes).Name);
        Assert.Equal(2, limiter.Calls);
    }

    private sealed class CountingLimiter : IRateLimiter
    {
        public int Calls;
        public Task<(bool isExceeded, int retryAfterMinutes)> CheckLimitAsync(string userId, string endpoint, int limit, CancellationToken ct = default)
        { Calls++; return Task.FromResult((false, 0)); }
    }
    private sealed class TestVault : GymBrain.Domain.Interfaces.IVaultService
    {
        public string Encrypt(string text) => text;
        public string Decrypt(string text) => text;
    }
    private sealed class FakeProvider : ILlmProvider, ILlmProviderFactory
    {
        public int Calls;
        public string LastMessage = ""; public string LastPrompt = "";
        public string ProviderName => "openai";
        public ILlmProvider GetProvider(string providerName) => this;
        public Task<string> ChatCompletionAsync(string apiKey, string model, string systemPrompt, string userMessage, bool forceJson = true, int maxTokens = 2048, CancellationToken ct = default)
        {
            Calls++; LastMessage = userMessage; LastPrompt = systemPrompt;
            return Task.FromResult("""{"components":[{"type":"set_tracker","payload":{"exercise_id":"10000001-0000-0000-0000-000000000013","weight_kg":150}}]}""");
        }
        public Task<IEnumerable<string>> GetAvailableModelsAsync(string apiKey, CancellationToken ct = default) => Task.FromResult(Enumerable.Empty<string>());
        public Task<bool> CheckHealthAsync(string apiKey, string model, CancellationToken ct = default) => Task.FromResult(true);
    }
    private sealed class MemoryCache : ICacheService
    {
        private readonly Dictionary<string, string> values = [];
        public Task<string?> GetAsync(string key, CancellationToken ct = default) => Task.FromResult(values.GetValueOrDefault(key));
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class => throw new NotSupportedException();
        public Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default) { values[key] = value; return Task.CompletedTask; }
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class => throw new NotSupportedException();
        public Task RemoveAsync(string key, CancellationToken ct = default) { values.Remove(key); return Task.CompletedTask; }
        public Task<long> IncrementAsync(string key, TimeSpan? expiry = null, CancellationToken ct = default) => Task.FromResult(1L);
    }

    private sealed class NoMilestones : IMilestoneService
    {
        public Task<List<Milestone>> EvaluateUnlocksAsync(Guid userId, CancellationToken ct = default) => Task.FromResult(new List<Milestone>());
    }
    private sealed class BeforeFirstSave(Func<Task> action) : SaveChangesInterceptor
    {
        private bool fired;
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!fired) { fired = true; await action(); }
            return result;
        }
    }
}
