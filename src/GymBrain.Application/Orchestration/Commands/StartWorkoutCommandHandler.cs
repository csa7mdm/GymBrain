using GymBrain.Application.Common;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Orchestration.Commands;

/// <summary>
/// Master orchestrator handler. Flow:
/// 1. Load user → decrypt BYO-API key
/// 2. Load seeded exercises → build token-compressed prompt
/// 3. Call LLM via ILlmProvider
/// 4. Run SafetyGate to sanitize output
/// 5. Cache sanitized payload in Redis (2hr TTL)
/// 6. Return SDUI mega-payload JSON
/// </summary>
public sealed class StartWorkoutCommandHandler(
    IApplicationDbContext db,
    IVaultService vault,
    ILlmProviderFactory llmProviderFactory,
    ICacheService cache)
    : IRequestHandler<StartWorkoutCommand, StartWorkoutResponse>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(2);

    public async Task<StartWorkoutResponse> Handle(StartWorkoutCommand request, CancellationToken ct)
    {
        // Check Redis cache first (avoid redundant LLM calls on reload)
        var cacheKey = $"workout:{request.UserId}:{request.ExperienceLevel}";
        var cached = await cache.GetAsync(cacheKey, ct);
        if (cached is not null)
            return new StartWorkoutResponse(cached);

        // Load user and decrypt BYO-API key
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        if (string.IsNullOrEmpty(user.EncryptedApiKey))
            throw new InvalidOperationException("No API key vaulted. Please vault your API key first.");

        var apiKey = vault.Decrypt(user.EncryptedApiKey);

        // Load exercises and build token-compressed prompt
        var exercises = await db.Exercises.ToListAsync(ct);
        if (exercises.Count == 0)
            throw new InvalidOperationException("No exercises available in the domain. Seed data may be missing.");

        var systemPrompt = SystemPromptFactory.Build(user.TonePersona, exercises);
        
        // Token-optimized user message with profile context
        var focusPart = string.IsNullOrWhiteSpace(request.WorkoutFocus) 
            ? "full-body" : request.WorkoutFocus;
        var levelStr = request.ExperienceLevel.ToString().ToLowerInvariant();
        var userMessage = $"Workout: {focusPart} | Level: {levelStr}";
        
        // Enrich with profile context if available
        if (!string.IsNullOrEmpty(user.Goal))
            userMessage += $" | Goal: {user.Goal}";
        if (!string.IsNullOrEmpty(user.EquipmentJson))
            userMessage += $" | Equipment: {user.EquipmentJson}";
        if (!string.IsNullOrEmpty(user.Injuries))
            userMessage += $" | Restrictions: {user.Injuries}";

        // Resolve provider and call LLM with fallback support for free models
        var provider = llmProviderFactory.GetProvider(user.LlmProvider ?? "openai");
        var preferredModel = user.PreferredModel ?? "gpt-4o-mini";
        
        // Define fallback sequence if the primary is a free model
        var modelsToTry = new List<string> { preferredModel };
        if (user.LlmProvider == "openrouter" || user.LlmProvider == "groq")
        {
            // Real-time Discovery: fetch what's actually available for this key
            var availableOnline = await provider.GetAvailableModelsAsync(apiKey, ct);
            var availableOnlineList = availableOnline.ToList();

            var otherFreeModels = LlmModelCatalog.GetByProvider(user.LlmProvider)
                .Where(m => m.IsFree && m.ModelId != preferredModel)
                .Select(m => m.ModelId)
                .Where(id => availableOnlineList.Count == 0 || availableOnlineList.Contains(id)); // intersection

            modelsToTry.AddRange(otherFreeModels);
        }

        string rawJson = string.Empty;
        Exception? lastException = null;

        foreach (var modelToTry in modelsToTry)
        {
            try
            {
                rawJson = await provider.ChatCompletionAsync(apiKey, modelToTry, systemPrompt, userMessage, forceJson: true, ct: ct);
                break; // Success!
            }
            catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("TooManyRequests") || ex.Message.Contains("404"))
            {
                lastException = ex;
                // Continue to next model in fallback list
                continue;
            }
        }

        if (string.IsNullOrEmpty(rawJson))
        {
            throw new InvalidOperationException(
                $"Failed to generate workout after trying {modelsToTry.Count} model(s). Last error: {lastException?.Message}", 
                lastException);
        }

        // Safety Gate: sanitize hallucinated IDs and clamp weights
        var safeJson = SafetyGate.Validate(rawJson, exercises, request.ExperienceLevel);

        // Cache for 2 hours (prevents redundant LLM calls on app reload)
        await cache.SetAsync(cacheKey, safeJson, CacheTtl, ct);

        return new StartWorkoutResponse(safeJson);
    }
}
