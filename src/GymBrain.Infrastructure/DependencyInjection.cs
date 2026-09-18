using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using GymBrain.Infrastructure.Persistence;
using GymBrain.Infrastructure.Providers;
using GymBrain.Infrastructure.Security;
using GymBrain.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace GymBrain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<GymBrainDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(GymBrainDbContext).Assembly.FullName))
            .ConfigureWarnings(w => w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<GymBrainDbContext>());

        // Delay connecting until a cache operation; Redis must not block API startup.
        var redisConn = configuration["REDIS_CONNECTION"]
            ?? configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(redisConn);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddScoped<RedisCacheService>();
        }
        services.AddScoped<ICacheService>(sp => new ResilientCacheService(
            sp.GetService<RedisCacheService>(),
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ResilientCacheService>>()));

        // Security services
        services.AddScoped<IVaultService, VaultService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IRateLimiter, RateLimiter>();
        services.AddScoped<IMilestoneService, MilestoneService>();

        // LLM Infrastructure
        services.AddScoped<ILlmProviderFactory, LlmProviderFactory>();

        // Register each provider with its own HttpClient
        services.AddHttpClient<ILlmProvider, OpenAiProvider>();
        services.AddHttpClient<ILlmProvider, GroqProvider>();
        services.AddHttpClient<ILlmProvider, OpenRouterProvider>();
        // Exercise Metadata Proxy
        services.AddHttpClient<IExerciseMetadataService, ExerciseMetadataProvider>();

        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret must be configured via env var or User Secrets.");

        var firebaseProject = configuration["Firebase:ProjectId"];
        var authentication = services.AddAuthentication("GymBrainAuth")
            .AddPolicyScheme("GymBrainAuth", "GymBrain or Firebase", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var header = context.Request.Headers.Authorization.ToString();
                    var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..] : "";
                    // This only selects a validator; neither scheme trusts these unverified claims.
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(firebaseProject) && token.Length < 16384 &&
                            new JwtSecurityTokenHandler().ReadJwtToken(token).Issuer == $"https://securetoken.google.com/{firebaseProject}")
                            return "Firebase";
                    }
                    catch (ArgumentException) { }
                    return JwtBearerDefaults.AuthenticationScheme;
                };
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var db = context.HttpContext.RequestServices.GetRequiredService<GymBrainDbContext>();
                        if (!Guid.TryParse(id, out var userId) || !await db.Users.AnyAsync(u => u.Id == userId && u.FirebaseUid == null, context.HttpContext.RequestAborted))
                            context.Fail("This session is no longer valid. Sign in again.");
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "GymBrain",
                    ValidAudience = configuration["Jwt:Audience"] ?? "GymBrain",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        if (!string.IsNullOrWhiteSpace(firebaseProject))
        {
            authentication.AddJwtBearer("Firebase", options =>
            {
                options.MapInboundClaims = false;
                options.Authority = $"https://securetoken.google.com/{firebaseProject}";
                options.Audience = firebaseProject;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, ValidIssuer = $"https://securetoken.google.com/{firebaseProject}",
                    ValidateAudience = true, ValidAudience = firebaseProject,
                    ValidateLifetime = true, ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true, RequireSignedTokens = true,
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }, ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal!;
                        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        if (!long.TryParse(principal.FindFirstValue("iat"), out var issued) || issued > now + 30 ||
                            !long.TryParse(principal.FindFirstValue("auth_time"), out var authenticated) || authenticated > now + 30)
                        { context.Fail("Invalid token time."); return; }
                        var db = context.HttpContext.RequestServices.GetRequiredService<GymBrainDbContext>();
                        var request = context.HttpContext.Request;
                        var isSignIn = request.Method == "POST" && request.Path == "/api/auth/firebase";
                        if (isSignIn)
                        {
                            if (!string.Equals(principal.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase))
                                context.Fail("Verify your email first.");
                            return;
                        }
                        var user = await FirebaseIdentity.ResolveAsync(db, principal, false, context.HttpContext.RequestAborted);
                        if (user == null) { context.Fail("Verify your email and sign in again."); return; }
                        // Application endpoints always receive the existing internal GUID, never a Firebase UID.
                        var identity = (ClaimsIdentity)principal.Identity!;
                        foreach (var claim in identity.FindAll(ClaimTypes.NameIdentifier).ToArray()) identity.RemoveClaim(claim);
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                    }
                };
            });
        }
        return services;
    }
}
