using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Orchestration.Commands;

public sealed class GenerateNutritionPlanCommandHandler(
    IApplicationDbContext db,
    IVaultService vault,
    ILlmProviderFactory llmProviderFactory)
    : IRequestHandler<GenerateNutritionPlanCommand, GenerateNutritionPlanResponse>
{
    public async Task<GenerateNutritionPlanResponse> Handle(GenerateNutritionPlanCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        if (string.IsNullOrEmpty(user.EncryptedApiKey))
            throw new InvalidOperationException("No API key vaulted. Please vault your API key first.");

        var apiKey = vault.Decrypt(user.EncryptedApiKey);

        var systemPrompt = NutritionPromptFactory.Build(user.TonePersona, request.Diet, request.Calories, request.Goal);
        var userMessage = $"Please generate a 1-day {request.Diet} meal plan for {request.Calories} calories focused on {request.Goal}.";

        var provider = llmProviderFactory.GetProvider(user.LlmProvider ?? "openai");
        var preferredModel = user.PreferredModel ?? "gpt-4o-mini";

        var rawJson = await provider.ChatCompletionAsync(apiKey, preferredModel, systemPrompt, userMessage, forceJson: true, ct: ct);

        return new GenerateNutritionPlanResponse(rawJson);
    }
}
