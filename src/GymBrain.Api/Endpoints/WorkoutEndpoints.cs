using System.Security.Claims;
using GymBrain.Application.Orchestration.Commands;
using GymBrain.Domain.Enums;
using MediatR;

namespace GymBrain.Api.Endpoints;

public static class WorkoutEndpoints
{
    public static void MapWorkoutEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/workout")
            .WithTags("Workout")
            .RequireAuthorization();

        group.MapPost("/start", async (StartWorkoutRequest request, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var levelClaim = user.FindFirstValue("experience_level");
            var level = Enum.TryParse<ExperienceLevel>(levelClaim, true, out var parsed)
                ? parsed
                : ExperienceLevel.Beginner;

            var command = new StartWorkoutCommand(userId, level, request.WorkoutFocus);
            var result = await sender.Send(command);

            return Results.Ok(result);
        })
        .WithName("StartWorkout");
    }
}

public record StartWorkoutRequest(string? WorkoutFocus);
