using Microsoft.EntityFrameworkCore;
using GymBrain.Infrastructure.Persistence;
using GymBrain.Api.Endpoints;
using Scalar.AspNetCore;
using Serilog;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Application;
using GymBrain.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 📊 Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Layer registration
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for React dev server and production frontend
builder.Services.AddCors(options =>
{
    var frontendUrls = builder.Configuration.GetSection("FrontendUrls").Get<string[]>()
        ?? new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:3000" };

    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(frontendUrls)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (GymBrain.Application.Common.Exceptions.RequestLimitException ex)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers.RetryAfter = (ex.RetryAfterMinutes * 60).ToString();
        await context.Response.WriteAsJsonAsync(new { detail = ex.Message, retryAfterMinutes = ex.RetryAfterMinutes });
    }
    catch (GymBrain.Application.Common.Exceptions.ManagedCapException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "You've used your 3 free AI workouts for today.",
            message = "Add your own API key in Profile -> Advanced -> Vault for unlimited workouts, or try again tomorrow.",
            retryAfterHours = ex.RetryAfterHours
        });
    }
    catch (GymBrain.Application.Common.Exceptions.CacheUnavailableException ex)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { detail = ex.Message });
    }
    catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested)
    {
        context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
        await context.Response.WriteAsJsonAsync(new { detail = "The request timed out. Please retry." });
    }
    catch (FluentValidation.ValidationException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { detail = "Request validation failed." });
    }
    catch (UnauthorizedAccessException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { detail = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { detail = ex.Message });
    }
    catch (ArgumentException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { detail = ex.Message });
    }
});

// Auto-migrate and optionally seed an admin when explicit credentials are configured.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GymBrainDbContext>();
    if (!app.Environment.IsEnvironment("Testing")) await db.Database.MigrateAsync();

    var adminEmail = builder.Configuration["SeedAdmin:Email"];
    var adminPassword = builder.Configuration["SeedAdmin:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hash = hasher.Hash(adminPassword);

        if (adminUser == null)
        {
            adminUser = new GymBrain.Domain.Entities.User(
                adminEmail,
                hash,
                GymBrain.Domain.Enums.ExperienceLevel.Beginner);
            db.Users.Add(adminUser);
        }
        else
        {
            adminUser.UpdatePassword(hash);
        }

        await db.SaveChangesAsync();
    }
}

app.MapOpenApi();
app.MapScalarApiReference();

// Health endpoints
app.MapGet("/", () => Results.Ok("GymBrain API is alive"))
    .WithName("HealthCheck")
    .WithTags("Health");

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthProbe")
    .WithTags("Health");

// Feature endpoints
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapWorkoutEndpoints();
app.MapNutritionEndpoints();
app.MapProfileEndpoints();
app.MapTelemetryEndpoints();

app.Run();

// Exposes the real HTTP pipeline to WebApplicationFactory integration tests.
public partial class Program { }
