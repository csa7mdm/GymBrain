using System.Security.Claims;
using GymBrain.Application.Orchestration.Commands;
using GymBrain.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

            var command = new GenerateNutritionPlanCommand(
                userId,
                request.Diet,
                request.Calories,
                request.Goal,
                request.DurationDays,
                request.MonthlyBudget,
                request.CurrencyCode,
                request.Country,
                request.City,
                request.AvailableResources,
                request.ReminderTime);
            var result = await sender.Send(command);

            return Results.Ok(result);
        })
        .WithName("GenerateNutritionPlan");

        group.MapGet("/latest", async (IApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));
            var plan = await db.NutritionPlans.AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAtUtc)
                .ThenByDescending(p => p.Id)
                .Select(p => new LatestNutritionPlanResponse(p.PayloadJson, p.CreatedAtUtc))
                .FirstOrDefaultAsync(ct);
            return plan is null ? Results.NoContent() : Results.Ok(plan);
        }).WithName("GetLatestNutritionPlan");
    }
}

public record GenerateNutritionRequest(
    string Diet,
    int Calories,
    string Goal,
    int DurationDays,
    decimal? MonthlyBudget,
    string? CurrencyCode,
    string? Country,
    string? City,
    string[]? AvailableResources,
    string? ReminderTime);

public record LatestNutritionPlanResponse(string PayloadJson, DateTime GeneratedAtUtc);
