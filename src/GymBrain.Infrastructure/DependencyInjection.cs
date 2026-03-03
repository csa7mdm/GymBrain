using System.Text;
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

        // Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "GymBrain:";
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // Security services
        services.AddScoped<IVaultService, VaultService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // LLM Infrastructure
        services.AddScoped<ILlmProviderFactory, LlmProviderFactory>();
        
        // Register each provider with its own HttpClient
        services.AddHttpClient<ILlmProvider, OpenAiProvider>();
        services.AddHttpClient<ILlmProvider, GroqProvider>();
        services.AddHttpClient<ILlmProvider, OpenRouterProvider>();
        services.AddHttpClient<ILlmProvider, AnthropicProvider>();

        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret must be configured via env var or User Secrets.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "GymBrain",
                    ValidAudience = configuration["Jwt:Issuer"] ?? "GymBrain",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });


        return services;
    }
}
