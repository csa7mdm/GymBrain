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

// CORS for React dev server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-migrate and seed on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GymBrainDbContext>();
    await db.Database.MigrateAsync();
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

app.Run();
