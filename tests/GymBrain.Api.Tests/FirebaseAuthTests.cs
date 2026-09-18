using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace GymBrain.Api.Tests;

public class FirebaseAuthTests
{
    const string Project = "gymbrain-pilot-cairo";
    const string Issuer = "https://securetoken.google.com/" + Project;
    [Fact]
    public async Task VerifiedIdentityRequiresOldPasswordToLinkAndPreservesAccountAndRevokesLegacySession()
    {
        await using var factory = new Factory(); using var client = factory.CreateClient();
        await factory.Initialize();
        var old = await client.PostAsJsonAsync("/api/auth/register", new { email = "Athlete@example.invalid", password = "OldPassword123!" });
        var account = await old.Content.ReadFromJsonAsync<JsonElement>();
        var id = account.GetProperty("userId").GetGuid();
        var oldToken = account.GetProperty("token").GetString();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GymBrainDbContext>();
            var user = await db.Users.SingleAsync(); user.IncrementWorkoutsCompleted(); await db.SaveChangesAsync();
        }
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.Token());
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/profile")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/firebase", new {})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/firebase", new { legacyPassword = "incorrect" })).StatusCode);
        var linked = await client.PostAsJsonAsync("/api/auth/firebase", new { legacyPassword = "OldPassword123!" });
        Assert.Equal(HttpStatusCode.OK, linked.StatusCode);
        Assert.Equal(id, (await linked.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetGuid());
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/firebase", new {})).StatusCode);
        var profile = await client.GetFromJsonAsync<JsonElement>("/api/profile");
        Assert.Equal(1, profile.GetProperty("workoutsCompleted").GetInt32());
        using (var scope = factory.Services.CreateScope())
        {
            var user = await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Users.SingleAsync();
            Assert.Equal("firebase-user", user.FirebaseUid);
            Assert.True(scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Verify("OldPassword123!", user.PasswordHash));
        }
        client.DefaultRequestHeaders.Authorization = new("Bearer", oldToken);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/profile")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { email = "Athlete@example.invalid", password = "OldPassword123!" })).StatusCode);
        // Another verified identity cannot take over a linked account, even with its former password.
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.Token(uid: "different-user"));
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/firebase", new { legacyPassword = "OldPassword123!" })).StatusCode);
    }

    [Theory]
    [InlineData("audience")][InlineData("issuer")][InlineData("expired")][InlineData("unverified")]
    [InlineData("signature")][InlineData("unsigned")][InlineData("future-auth")]
    public async Task RejectsInvalidFirebaseTokens(string failure)
    {
        await using var factory = new Factory(); using var client = factory.CreateClient(); await factory.Initialize();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.Token(failure));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/firebase", new {})).StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Users.ToListAsync());
    }
    [Fact]
    public async Task NewVerifiedUserIsCreatedOnceAndOtherUserCannotReadProfile()
    {
        await using var factory = new Factory(); using var client = factory.CreateClient(); await factory.Initialize();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.Token());
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/firebase", new {})).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/firebase", new {})).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/profile")).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.Token(uid: "stranger"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/profile")).StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.Single(await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Users.ToListAsync());
    }
    private sealed class Factory : WebApplicationFactory<Program>
    {
        readonly SqliteConnection connection = new("Data Source=:memory:");
        readonly RSA rsa = RSA.Create(2048);
        public Factory() { connection.Open(); }
        public async Task Initialize() { using var scope = Services.CreateScope(); await scope.ServiceProvider.GetRequiredService<GymBrainDbContext>().Database.EnsureCreatedAsync(); }
        public string Token(string failure = "", string uid = "firebase-user")
        {
            var now = DateTimeOffset.UtcNow;
            using var other = RSA.Create(2048);
            var key = new RsaSecurityKey(failure == "signature" ? other : rsa) { KeyId = "test-key" };
            var token = new JwtSecurityToken(failure == "issuer" ? "https://attacker.invalid" : Issuer,
                failure == "audience" ? "other-project" : Project,
                new[] { new Claim("sub", uid), new Claim("email", "athlete@example.invalid"),
                    new Claim("email_verified", failure == "unverified" ? "false" : "true", ClaimValueTypes.Boolean),
                    new Claim("iat", now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim("auth_time", (failure == "future-auth" ? now.AddHours(1) : now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) },
                now.AddHours(-2).UtcDateTime, (failure == "expired" ? now.AddHours(-1) : now.AddHours(1)).UtcDateTime,
                failure == "unsigned" ? null : new SigningCredentials(key, SecurityAlgorithms.RsaSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing"); builder.UseSetting("Jwt:Secret", "firebase-test-only-secret-at-least-32-bytes");
            builder.UseSetting("Firebase:ProjectId", Project); builder.UseSetting("SeedAdmin:Email", ""); builder.UseSetting("SeedAdmin:Password", "");
            builder.ConfigureTestServices(services => {
                services.RemoveAll<DbContextOptions<GymBrainDbContext>>(); services.RemoveAll<IDbContextOptionsConfiguration<GymBrainDbContext>>();
                services.AddDbContext<GymBrainDbContext>(options => options.UseSqlite(connection));
                services.Configure<JwtBearerOptions>("Firebase", options => {
                    // Keep the real signature/issuer/audience/lifetime validator; replace only public-key discovery.
                    var config = new OpenIdConnectConfiguration { Issuer = Issuer };
                    config.SigningKeys.Add(new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = "test-key" });
                    options.Configuration = config;
                });
            });
        }
        public override async ValueTask DisposeAsync() { await base.DisposeAsync(); await connection.DisposeAsync(); rsa.Dispose(); }
    }
}
