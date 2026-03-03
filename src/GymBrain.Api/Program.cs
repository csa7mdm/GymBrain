using GymBrain.Api.Endpoints;
using GymBrain.Application;
using GymBrain.Infrastructure;
using GymBrain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Layer registration
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapWorkoutEndpoints();

app.Run();
