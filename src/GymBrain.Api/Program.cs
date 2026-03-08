using GymBrain.Api.Endpoints;
using GymBrain.API.Endpoints;
using GymBrain.Application;
using GymBrain.Infrastructure;
using GymBrain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Layer registration
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for React dev server and Production frontend
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

app.Use(async (context, next) =>
{
    try
    {
        await next();
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

// Auto-migrate and seed on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GymBrainDbContext>();
    await db.Database.MigrateAsync();

    var adminEmail = "cs.a7md.m@gmail.com";
    var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
    var hasher = scope.ServiceProvider.GetRequiredService<GymBrain.Application.Common.Interfaces.IPasswordHasher>();
    var hash = hasher.Hash("P@$sW0Rdz9090");

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
        // Force update password to ensure matches requested default
        adminUser.UpdatePassword(hash);
    }
    await db.SaveChangesAsync();
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
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapWorkoutEndpoints();
app.MapNutritionEndpoints();
app.MapProfileEndpoints();
app.MapTelemetryEndpoints();

app.Run();
