using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Orchestration.Commands;

public sealed class GenerateNutritionPlanCommandHandler(
    IApplicationDbContext db,
    IVaultService vault,
    ILlmProviderFactory llmProviderFactory,
    ICacheService cache,
    Microsoft.Extensions.Configuration.IConfiguration configuration)
    : IRequestHandler<GenerateNutritionPlanCommand, GenerateNutritionPlanResponse>
{
    private const string ManagedLlmProvider = "groq";
    private const string ManagedLlmModel = "llama-3.3-70b-versatile";
    private const int ManagedDailyCapPerUser = 3;

    public async Task<GenerateNutritionPlanResponse> Handle(GenerateNutritionPlanCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        string apiKey;
        string providerName;
        string preferredModel;

        if (!string.IsNullOrEmpty(user.EncryptedApiKey))
        {
            apiKey = vault.Decrypt(user.EncryptedApiKey);
            providerName = user.LlmProvider ?? "openai";
            preferredModel = user.PreferredModel ?? "gpt-4o-mini";
        }
        else
        {
            var dailyCapKey = $"managed_nutr_cap:{request.UserId}:{DateTime.UtcNow:yyyy-MM-dd}";
            var currentCountStr = await cache.GetAsync(dailyCapKey, ct);
            var currentCount = currentCountStr is not null ? int.Parse(currentCountStr) : 0;

            if (currentCount >= ManagedDailyCapPerUser)
            {
                var now = DateTime.UtcNow;
                var midnight = now.Date.AddDays(1);
                var hoursUntilMidnight = (int)(midnight - now).TotalHours;
                throw new GymBrain.Application.Common.Exceptions.ManagedCapException("Daily AI limit reached for meal planning.", Math.Max(1, hoursUntilMidnight));
            }

            var managedKey = configuration["GYMBRAIN_MANAGED_LLM_KEY"]
                ?? Environment.GetEnvironmentVariable("GYMBRAIN_MANAGED_LLM_KEY")
                ?? throw new InvalidOperationException("Managed LLM key not configured. Please add your API key in the Vault.");

            apiKey = managedKey;
            providerName = ManagedLlmProvider;
            preferredModel = ManagedLlmModel;

            await cache.IncrementAsync(dailyCapKey, TimeSpan.FromDays(1), ct);
        }

        var durationDays = Math.Clamp(request.DurationDays, 1, 31);
        var systemPrompt = NutritionPromptFactory.Build(
            user.TonePersona,
            request.Diet,
            request.Calories,
            request.Goal,
            durationDays,
            request.MonthlyBudget,
            request.CurrencyCode,
            request.Country,
            request.City,
            request.AvailableResources,
            request.ReminderTime);

        var locationLine = string.Join(", ", new[] { request.City, request.Country }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (string.IsNullOrWhiteSpace(locationLine))
            locationLine = "the user's region";

        var resourcesLine = request.AvailableResources is { Length: > 0 }
            ? string.Join(", ", request.AvailableResources)
            : "standard home cooking resources";

        var userMessage = $"Generate a {durationDays}-day {request.Diet} meal plan for {request.Calories} daily calories focused on {request.Goal}. Use a monthly budget of {request.MonthlyBudget?.ToString("0.##") ?? "unspecified"} {request.CurrencyCode ?? "local currency"}, assume the user is in {locationLine}, and only rely on these available resources: {resourcesLine}.";

        var provider = llmProviderFactory.GetProvider(providerName);
        var rawJson = await provider.ChatCompletionAsync(apiKey, preferredModel, systemPrompt, userMessage, forceJson: true, ct: ct);

        return new GenerateNutritionPlanResponse(rawJson);
    }
}
