using GymBrain.Application.Profile.Commands;
using GymBrain.Application.Profile.Queries;
using MediatR;
using System.Security.Claims;

namespace GymBrain.API.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/profile").RequireAuthorization();

        group.MapPost("/save", async (SaveProfileRequest request, IMediator mediator, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var command = new SaveProfileCommand(
                userId,
                request.Goal,
                request.EquipmentJson,
                request.Injuries,
                request.DaysPerWeek,
                request.DietaryPreference,
                request.DailyCalories,
                request.ExperienceLevel);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        });

        group.MapGet("/", async (IMediator mediator, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await mediator.Send(new GetProfileQuery(userId));
            return Results.Ok(result);
        });
    }
}

public record SaveProfileRequest(
    string? Goal,
    string? EquipmentJson,
    string? Injuries,
    int DaysPerWeek,
    string? DietaryPreference,
    int DailyCalories,
    string ExperienceLevel);
