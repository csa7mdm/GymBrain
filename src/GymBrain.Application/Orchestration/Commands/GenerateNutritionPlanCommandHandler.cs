using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using GymBrain.Domain.Entities;
using MediatR;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        if (request.DurationDays is < 1 or > 7)
            throw new ArgumentException("Choose between 1 and 7 days.");
        if (request.DailyBudget is <= 0 or > 1000000)
            throw new ArgumentException("Daily budget must be greater than zero and at most 1,000,000.");
        if (request.DailyBudget.HasValue && (request.CurrencyCode is null ||
            !System.Text.RegularExpressions.Regex.IsMatch(request.CurrencyCode, "^[A-Z]{3}$")))
            throw new ArgumentException("Choose a three-letter currency for your daily budget.");
        if (request.PreferredItems is { Length: > 20 } ||
            request.PreferredItems?.Any(item => string.IsNullOrWhiteSpace(item) || item.Length > 80) == true)
            throw new ArgumentException("Use up to 20 preferred items, each at most 80 characters.");
        if (request.Restrictions?.Length > 500)
            throw new ArgumentException("Keep restrictions within 500 characters.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(85));
        ct = timeout.Token;
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
            var currentCount = await cache.IncrementAsync(dailyCapKey, TimeSpan.FromDays(1), ct);

            if (currentCount > ManagedDailyCapPerUser)
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

        }

        var durationDays = request.DurationDays;
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
            request.ReminderTime,
            request.DailyBudget,
            request.PreferredItems,
            request.Restrictions);

        var userMessage = $"Generate the requested {durationDays}-day meal plan. Apply the planning preferences and restrictions in the system prompt. Return all requested days as complete JSON.";
        var provider = llmProviderFactory.GetProvider(providerName);
        // Longer plans need more output room; keep the existing one-day budget.
        var maxTokens = Math.Min(16000, 6000 + (durationDays - 1) * 1800);
        var rawJson = await provider.ChatCompletionAsync(apiKey, preferredModel, systemPrompt, userMessage, forceJson: true, maxTokens: maxTokens, ct: ct);
        var payloadJson = NutritionPlanPayload.ValidateAndExtract(rawJson, durationDays);
        var payload = JsonNode.Parse(payloadJson)!.AsObject();
        // Save the user's actual settings, not model-reported preferences.
        payload["planning_preferences"] = JsonSerializer.SerializeToNode(new {
            durationDays, dailyBudget = request.DailyBudget,
            currencyCode = request.DailyBudget.HasValue ? request.CurrencyCode : null,
            preferredItems = request.PreferredItems ?? [], restrictions = request.Restrictions?.Trim()
        });
        payloadJson = payload.ToJsonString();
        db.NutritionPlans.Add(new NutritionPlan(request.UserId, payloadJson));
        await db.SaveChangesAsync(ct);
        return new GenerateNutritionPlanResponse(payloadJson);
    }
}
