using GymBrain.Application.Common.Interfaces;
using GymBrain.Application.Orchestration.Commands;
using MediatR;
using System.Security.Claims;

namespace GymBrain.Api.Endpoints;

public static class WorkoutEndpoints
{
    public static void MapWorkoutEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/workout")
            .WithTags("Workout")
            .RequireAuthorization();

        group.MapPost("/start", async (StartWorkoutRequest request, ISender sender, ClaimsPrincipal user, IRateLimiter rateLimiter) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var levelClaim = user.FindFirstValue("experience_level");
            var level = Enum.TryParse<Domain.Enums.ExperienceLevel>(levelClaim, true, out var parsed)
                ? parsed
                : Domain.Enums.ExperienceLevel.Beginner;

            // === RATE LIMITING (Task 1D): max 10 workout generations per user per hour ===
            var (isExceeded, retryAfter) = await rateLimiter.CheckLimitAsync(userId.ToString(), "workout_start", 10);
            if (isExceeded)
                return Results.Json(new { error = $"Rate limit exceeded. Try again in {retryAfter} minutes.", retryAfterMinutes = retryAfter }, statusCode: 429);

            var command = new StartWorkoutCommand(userId, level, request.WorkoutFocus);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("StartWorkout");

        group.MapPost("/save", async (SaveWorkoutRequest request, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var command = new SaveWorkoutCommand(userId, request.PayloadJson);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("SaveWorkout");

        // === POST /api/workout/substitute (Task 1B — Machine Taken) ===
        group.MapPost("/substitute", async (SubstituteRequest request, ISender sender, ClaimsPrincipal user, IRateLimiter rateLimiter, Application.Common.Interfaces.IApplicationDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            // Rate limiting: max 30 substitute requests per user per hour
            var (isExceeded, retryAfter) = await rateLimiter.CheckLimitAsync(userId.ToString(), "substitute", 30);
            if (isExceeded)
                return Results.Json(new { error = $"Rate limit exceeded. Try again in {retryAfter} minutes.", retryAfterMinutes = retryAfter }, statusCode: 429);

            // Load user injuries from profile
            var userEntity = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(db.Users, u => u.Id == userId, default);
            var injuries = userEntity?.Injuries;

            if (!Guid.TryParse(request.ExerciseId, out var exerciseGuid))
                return Results.BadRequest("Invalid exerciseId format.");

            var query = new GetSubstituteQuery(userId, exerciseGuid, injuries);
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetSubstitute");

        group.MapGet("/exercise-metadata/{name}", async (string name, ISender sender) =>
        {
            var query = new Application.Workout.Queries.GetExerciseMetadataQuery(name);
            var result = await sender.Send(query);
            return result is not null ? Results.Ok(result) : Results.NoContent();
        })
        .WithName("GetExerciseMetadata");
    }
}

public record StartWorkoutRequest(string? WorkoutFocus);
public record SaveWorkoutRequest(string PayloadJson);
public record SubstituteRequest(string ExerciseId);
