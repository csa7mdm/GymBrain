using GymBrain.Application.Common.Interfaces;
using GymBrain.Application.Orchestration.Commands;
using MediatR;
using System.Security.Claims;

namespace GymBrain.API.Endpoints;

public static class WorkoutEndpoints
{
    public static void MapWorkoutEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/workout")
            .WithTags("Workout")
            .RequireAuthorization();

        group.MapPost("/start", async (StartWorkoutRequest request, ISender sender, ClaimsPrincipal user, ICacheService cache) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var levelClaim = user.FindFirstValue("experience_level");
            var level = Enum.TryParse<Domain.Enums.ExperienceLevel>(levelClaim, true, out var parsed)
                ? parsed
                : Domain.Enums.ExperienceLevel.Beginner;

            // === RATE LIMITING (Task 1D): max 10 workout generations per user per hour ===
            var hourKey = $"ratelimit:{userId}:workout_start:{DateTime.UtcNow:yyyyMMddHH}";
            var countStr = await cache.GetAsync(hourKey, default);
            var count = countStr is not null ? int.Parse(countStr) : 0;
            if (count >= 10)
                return Results.StatusCode(429);

            await cache.SetAsync(hourKey, (count + 1).ToString(), TimeSpan.FromHours(1), default);

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
        group.MapPost("/substitute", async (SubstituteRequest request, ISender sender, ClaimsPrincipal user, ICacheService cache, Application.Common.Interfaces.IApplicationDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            // Rate limiting: max 30 substitute requests per user per hour
            var hourKey = $"ratelimit:{userId}:substitute:{DateTime.UtcNow:yyyyMMddHH}";
            var countStr = await cache.GetAsync(hourKey, default);
            var count = countStr is not null ? int.Parse(countStr) : 0;
            if (count >= 30)
                return Results.StatusCode(429);

            await cache.SetAsync(hourKey, (count + 1).ToString(), TimeSpan.FromHours(1), default);

            // Load user injuries from profile
            var userEntity = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(db.Users, u => u.Id == userId, default);
            var injuries = userEntity?.Injuries;

            if (!Guid.TryParse(request.ExerciseId, out var exerciseGuid))
                return Results.BadRequest("Invalid exerciseId format.");

            var query = new GetSubstituteQuery(exerciseGuid, injuries);
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetSubstitute");
    }
}

public record StartWorkoutRequest(string? WorkoutFocus);
public record SaveWorkoutRequest(string PayloadJson);
public record SubstituteRequest(string ExerciseId);
