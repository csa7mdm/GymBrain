using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using System.Text.Json;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Infrastructure.Persistence;
using GymBrain.Infrastructure.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GymBrain.Api.Tests;

public sealed class WorkoutJourneyTests
{
    [Fact]
    public async Task ProviderFailureReturnsSafeCorsReadableError()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Database.EnsureCreatedAsync();
        var account = await Read(await client.PostAsJsonAsync("/api/auth/register", new {
            email = "provider-error@example.invalid", password = "TestPassword123!"
        }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.GetProperty("token").GetString());
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:5173");
        factory.Provider.Fail = true;

        var response = await client.PostAsJsonAsync("/api/nutrition/generate", new {
            diet = "Standard", calories = 2000, goal = "strength", durationDays = 1
        });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("another model", body);
        Assert.DoesNotContain("StackTrace", body);
    }

    [Fact]
    public async Task RealHttpJourneyPersistsCompletionAcrossLoginAndEnforcesOwnershipAndLimits()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Database.EnsureCreatedAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/workout/history")).StatusCode);
        var account = await Read(await client.PostAsJsonAsync("/api/auth/register", new { email = "athlete@example.invalid", password = "TestPassword123!" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.GetProperty("token").GetString());
        await Read(await client.PostAsJsonAsync("/api/profile/save", new {
            goal = "strength", equipmentJson = "[\"Dumbbells\"]", injuries = "knee pain", daysPerWeek = 3,
            dietaryPreference = "Standard", dailyCalories = 2000, experienceLevel = "Advanced"
        }));
        // The JWT still says Beginner; generation must read Advanced from the profile.
        var generated = await Read(await client.PostAsJsonAsync("/api/workout/start", new { workoutFocus = "core" }));
        Assert.Contains("Level: advanced", factory.Provider.LastMessage);
        Assert.DoesNotContain("Goblet Squat", factory.Provider.LastPrompt);
        Assert.True(JsonDocument.Parse(generated.GetProperty("megaPayloadJson").GetString()!).RootElement.GetProperty("components").GetArrayLength() > 0);
        Assert.Single(factory.Cache.Counts, pair => pair.Key.Contains(":workout_start:") && pair.Value == 1);

        var sessionId = Guid.NewGuid();
        var payload = """{"schemaVersion":1,"focus":"Core","exercises":[{"name":"Plank","sets":[{"completed":true,"reps":8,"weightKg":0}]}]}""";
        for (var i = 0; i < 2; i++)
            await Read(await client.PostAsJsonAsync("/api/workout/save", new { sessionId, payloadJson = payload }));
        client.DefaultRequestHeaders.Authorization = null;
        var login = await Read(await client.PostAsJsonAsync("/api/auth/login", new { email = "athlete@example.invalid", password = "TestPassword123!" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.GetProperty("token").GetString());
        var history = await Read(await client.GetAsync("/api/workout/history"));
        Assert.Equal(1, history.GetProperty("total").GetInt32());
        Assert.Equal(payload, history.GetProperty("items")[0].GetProperty("payloadJson").GetString());
        var profile = await Read(await client.GetAsync("/api/profile"));
        Assert.Equal(1, profile.GetProperty("workoutsCompleted").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/metrics")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/workout/save", new { sessionId = Guid.NewGuid(), payloadJson = "{}" })).StatusCode);

        // Cached generation still consumes exactly one hourly allowance, not two.
        for (var i = 0; i < 9; i++) await Read(await client.PostAsJsonAsync("/api/workout/start", new { workoutFocus = "core" }));
        var limited = await client.PostAsJsonAsync("/api/workout/start", new { workoutFocus = "core" });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.NotNull(limited.Headers.RetryAfter);

        client.DefaultRequestHeaders.Authorization = null;
        var second = await Read(await client.PostAsJsonAsync("/api/auth/register", new { email = "other@example.invalid", password = "TestPassword123!" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", second.GetProperty("token").GetString());
        Assert.Equal(0, (await Read(await client.GetAsync("/api/workout/history"))).GetProperty("total").GetInt32());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/workout/save", new { sessionId, payloadJson = payload })).StatusCode);
    }

    [Fact]
    public async Task HourlyLimitAtMidnightReportsMinutesRatherThanAnExtraDay()
    {
        var limiter = new GymBrain.Infrastructure.Services.RateLimiter(new TestCache(), new MidnightClock());
        var result = await limiter.CheckLimitAsync("test", "workout_start", 0);
        Assert.True(result.isExceeded);
        Assert.Equal(1, result.retryAfterMinutes);
    }
    private sealed class MidnightClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 9, 17, 23, 59, 30, TimeSpan.Zero);
    }

    [Fact]
    public async Task PersonalProfileSurvivesLoginAndLegacyWritesAndIsAccountScoped()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Database.EnsureCreatedAsync();
        var account = await Read(await client.PostAsJsonAsync("/api/auth/register", new { email = "profile@example.invalid", password = "TestPassword123!" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.GetProperty("token").GetString());
        var preferences = new { goal = "strength", equipmentJson = "[]", injuries = "", daysPerWeek = 3, dietaryPreference = "Standard", dailyCalories = 2000, experienceLevel = "Advanced" };
        await Read(await client.PostAsJsonAsync("/api/profile/save", new {
            preferences.goal, preferences.equipmentJson, preferences.injuries, preferences.daysPerWeek, preferences.dietaryPreference, preferences.dailyCalories, preferences.experienceLevel,
            personalProfile = new { name = " Test Athlete ", age = 34, height = 180.5, weight = 82.5, focusAreas = new[] { "Back", "Core" } }
        }));
        // An older client cannot erase fields it does not know about.
        await Read(await client.PostAsJsonAsync("/api/profile/save", preferences));
        client.DefaultRequestHeaders.Authorization = null;
        var login = await Read(await client.PostAsJsonAsync("/api/auth/login", new { email = "profile@example.invalid", password = "TestPassword123!" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.GetProperty("token").GetString());
        var personal = (await Read(await client.GetAsync("/api/profile"))).GetProperty("personalProfile");
        Assert.Equal("Test Athlete", personal.GetProperty("name").GetString());
        Assert.Equal(34, personal.GetProperty("age").GetInt32());
        Assert.Equal(180.5, personal.GetProperty("height").GetDouble());
        Assert.Equal(82.5, personal.GetProperty("weight").GetDouble());
        Assert.Equal(new[] { "Back", "Core" }, personal.GetProperty("focusAreas").EnumerateArray().Select(x => x.GetString()));
        client.DefaultRequestHeaders.Authorization = null;
        var second = await Read(await client.PostAsJsonAsync("/api/auth/register", new { email = "separate@example.invalid", password = "TestPassword123!" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", second.GetProperty("token").GetString());
        Assert.Equal(JsonValueKind.Null, (await Read(await client.GetAsync("/api/profile"))).GetProperty("personalProfile").ValueKind);
    }

    [Theory]
    [InlineData(0, 180, 80, "Core")]
    [InlineData(30, 0, 80, "Core")]
    [InlineData(30, 180, -1, "Core")]
    [InlineData(30, 180, 80, "Invalid")]
    public async Task InvalidPersonalProfileDoesNotSavePartialPreferences(int age, double height, double weight, string focus)
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Database.EnsureCreatedAsync();
        var account = await Read(await client.PostAsJsonAsync("/api/auth/register", new { email = "invalid@example.invalid", password = "TestPassword123!" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.GetProperty("token").GetString());
        var response = await client.PostAsJsonAsync("/api/profile/save", new {
            goal = "strength", daysPerWeek = 4, experienceLevel = "Advanced",
            personalProfile = new { name = "Athlete", age, height, weight, focusAreas = new[] { focus } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var profile = await Read(await client.GetAsync("/api/profile"));
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("personalProfile").ValueKind);
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("goal").ValueKind);
    }

    private static async Task<JsonElement> Read(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    private sealed class Factory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public TestCache Cache { get; } = new();
        public TestProvider Provider { get; } = new();
        public Factory() { connection.Open(); }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Secret", "integration-test-only-secret-at-least-32-bytes-long");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> {
                ["Jwt:Secret"] = "integration-test-only-secret-at-least-32-bytes-long",
                ["Vault:EncryptionKey"] = Convert.ToBase64String(new byte[32]),
                ["GYMBRAIN_MANAGED_LLM_KEY"] = "test-only-provider-key",
                ["ConnectionStrings:Redis"] = "", ["REDIS_CONNECTION"] = "",
                ["SeedAdmin:Email"] = "", ["SeedAdmin:Password"] = ""
            }));
            builder.ConfigureTestServices(services => {
                services.RemoveAll<DbContextOptions<GymBrainDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<GymBrainDbContext>>();
                services.AddDbContext<GymBrainDbContext>(options => options.UseSqlite(connection));
                services.RemoveAll<ICacheService>(); services.AddSingleton<ICacheService>(Cache);
                services.RemoveAll<ILlmProviderFactory>(); services.AddSingleton<ILlmProviderFactory>(Provider);
            });
        }
        public override async ValueTask DisposeAsync() { await base.DisposeAsync(); await connection.DisposeAsync(); }
    }
    private sealed class TestProvider : ILlmProviderFactory, ILlmProvider
    {
        public string LastMessage = ""; public string LastPrompt = "";
        public bool Fail;
        public string ProviderName => "groq";
        public ILlmProvider GetProvider(string name) => this;
        public Task<string> ChatCompletionAsync(string apiKey, string model, string systemPrompt, string userMessage, bool forceJson = true, int maxTokens = 2048, CancellationToken ct = default)
        { if (Fail) throw new ProviderResponseException("Choose another model in Vault."); LastMessage = userMessage; LastPrompt = systemPrompt; return Task.FromResult("""{"components":[{"type":"set_tracker","payload":{"exercise_id":"10000001-0000-0000-0000-000000000013","sets":2,"reps":8,"weight_kg":0}}]}"""); }
        public Task<IEnumerable<string>> GetAvailableModelsAsync(string apiKey, CancellationToken ct = default) => Task.FromResult(Enumerable.Empty<string>());
        public Task<bool> CheckHealthAsync(string apiKey, string model, CancellationToken ct = default) => Task.FromResult(true);
    }
    private sealed class TestCache : ICacheService
    {
        public ConcurrentDictionary<string, long> Counts { get; } = new();
        private readonly ConcurrentDictionary<string, string> values = new();
        public Task<string?> GetAsync(string key, CancellationToken ct = default) => Task.FromResult(values.GetValueOrDefault(key));
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class => throw new NotSupportedException();
        public Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default) { values[key] = value; return Task.CompletedTask; }
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class => throw new NotSupportedException();
        public Task RemoveAsync(string key, CancellationToken ct = default) { values.TryRemove(key, out _); return Task.CompletedTask; }
        public Task<long> IncrementAsync(string key, TimeSpan? expiry = null, CancellationToken ct = default) => Task.FromResult(Counts.AddOrUpdate(key, 1, (_, old) => old + 1));
    }
}
