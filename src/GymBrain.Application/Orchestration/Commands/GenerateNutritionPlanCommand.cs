using MediatR;

namespace GymBrain.Application.Orchestration.Commands;

public sealed record GenerateNutritionPlanCommand(
    Guid UserId,
    string Diet,
    int Calories,
    string Goal,
    int DurationDays,
    decimal? MonthlyBudget,
    string? CurrencyCode,
    string? Country,
    string? City,
    string[]? AvailableResources,
    string? ReminderTime) : IRequest<GenerateNutritionPlanResponse>;

public sealed record GenerateNutritionPlanResponse(string PayloadJson);
