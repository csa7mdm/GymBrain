using System.Security.Claims;
using GymBrain.Application.Orchestration.Commands;
using MediatR;

namespace GymBrain.Api.Endpoints;

public static class NutritionEndpoints
{
    public static void MapNutritionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/nutrition")
            .WithTags("Nutrition")
            .RequireAuthorization();

        group.MapPost("/generate", async (GenerateNutritionRequest request, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var command = new GenerateNutritionPlanCommand(userId, request.Diet, request.Calories, request.Goal);
            var result = await sender.Send(command);

            return Results.Ok(result);
        })
        .WithName("GenerateNutritionPlan");
    }
}

public record GenerateNutritionRequest(string Diet, int Calories, string Goal);
