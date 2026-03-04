using MediatR;

namespace GymBrain.Application.Orchestration.Commands;

public sealed record GenerateNutritionPlanCommand(
    Guid UserId,
    string Diet,
    int Calories,
    string Goal) : IRequest<GenerateNutritionPlanResponse>;

public sealed record GenerateNutritionPlanResponse(string PayloadJson);
