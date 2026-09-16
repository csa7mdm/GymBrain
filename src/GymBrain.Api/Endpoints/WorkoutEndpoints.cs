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

        group.MapPost("/start", async (StartWorkoutRequest request, ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var levelClaim = user.FindFirstValue("experience_level");
            var level = Enum.TryParse<Domain.Enums.ExperienceLevel>(levelClaim, true, out var parsed)
                ? parsed
                : Domain.Enums.ExperienceLevel.Beginner;

            var command = new StartWorkoutCommand(userId, level, request.WorkoutFocus);
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("StartWorkout");

        group.MapPost("/save", async (SaveWorkoutRequest request, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var command = new SaveWorkoutCommand(userId, request.PayloadJson, request.SessionId);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("SaveWorkout");

        group.MapGet("/history", async (int? offset, ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Invalid token."));
            return Results.Ok(await sender.Send(new Application.Orchestration.Queries.GetWorkoutHistoryQuery(userId, offset ?? 0), ct));
        }).WithName("GetWorkoutHistory");

        // === POST /api/workout/substitute (Task 1B — Machine Taken) ===
        group.MapPost("/substitute", async (SubstituteRequest request, ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            if (!Guid.TryParse(request.ExerciseId, out var exerciseGuid))
                return Results.BadRequest("Invalid exerciseId format.");

            var query = new GetSubstituteQuery(userId, exerciseGuid, null);
            var result = await sender.Send(query, ct);
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
public record SaveWorkoutRequest(string PayloadJson, Guid? SessionId = null);
public record SubstituteRequest(string ExerciseId);
