using GymBrain.Application.Common;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GymBrain.Application.Orchestration.Commands;

/// <summary>
/// Master orchestrator handler. Flow:
/// 1. Load user → decrypt BYO-API key (or use managed key)
/// 2. Check Redis cache
/// 3. Load exercises → FILTER by injury contraindications (deterministic, pre-LLM)
/// 4. Build token-compressed prompt from SAFE catalog only
/// 5. Call LLM via ILlmProvider with fallback model support
/// 6. Run SafetyGate to sanitize output
/// 7. Prepend warm-up block (deterministic)
/// 8. Cache sanitized payload in Redis (2hr TTL)
/// 9. Return SDUI mega-payload JSON
/// </summary>
public sealed class StartWorkoutCommandHandler(
    IApplicationDbContext db,
    IVaultService vault,
    ILlmProviderFactory llmProviderFactory,
    ICacheService cache,
    Microsoft.Extensions.Configuration.IConfiguration configuration)
    : IRequestHandler<StartWorkoutCommand, StartWorkoutResponse>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(2);
    private const string ManagedLlmProvider = "groq";
    private const string ManagedLlmModel = "llama-3.3-70b-versatile";
    private const int ManagedDailyCapPerUser = 3;

    public async Task<StartWorkoutResponse> Handle(StartWorkoutCommand request, CancellationToken ct)
    {
        // Check Redis cache first (avoid redundant LLM calls on reload)
        var cacheKey = $"workout:{request.UserId}:{request.ExperienceLevel}";
        var cached = await cache.GetAsync(cacheKey, ct);
        if (cached is not null)
            return new StartWorkoutResponse(cached);

        // Load user 
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        // Resolve API key: BYO key takes priority; fall back to managed key
        string apiKey;
        string providerName;
        string preferredModel;

        if (!string.IsNullOrEmpty(user.EncryptedApiKey))
        {
            // BYO mode — user has vaulted their own key
            apiKey = vault.Decrypt(user.EncryptedApiKey);
            providerName = user.LlmProvider ?? "openai";
            preferredModel = user.PreferredModel ?? "gpt-4o-mini";
        }
        else
        {
            // Managed mode — check daily cap first
            var dailyCapKey = $"managed_limit:{request.UserId}:{DateTime.UtcNow:yyyy-MM-dd}";
            var currentCountStr = await cache.GetAsync(dailyCapKey, ct);
            var currentCount = currentCountStr is not null ? int.Parse(currentCountStr) : 0;

            if (currentCount >= ManagedDailyCapPerUser)
                throw new InvalidOperationException(
                    "Daily AI limit reached. Add your own API key in the Vault for unlimited workouts, or try again tomorrow.");

            var managedKey = configuration["GYMBRAIN_MANAGED_LLM_KEY"]
                ?? Environment.GetEnvironmentVariable("GYMBRAIN_MANAGED_LLM_KEY")
                ?? throw new InvalidOperationException("Managed LLM key not configured. Please add your API key in the Vault.");

            apiKey = managedKey;
            providerName = ManagedLlmProvider;
            preferredModel = ManagedLlmModel;

            // Increment cap counter (TTL 24h)
            await cache.SetAsync(dailyCapKey, (currentCount + 1).ToString(), TimeSpan.FromDays(1), ct);
        }

        // Load ALL exercises (including warmups)
        var allExercises = await db.Exercises.ToListAsync(ct);
        if (allExercises.Count == 0)
            throw new InvalidOperationException("No exercises available. Seed data may be missing.");

        // === DETERMINISTIC INJURY PRE-FILTER (Task 1A) ===
        // Filters out warmup exercises AND contraindicated exercises BEFORE LLM call
        var safeExercises = InjuryFilter.Filter(allExercises, user.Injuries);
        if (safeExercises.Count == 0)
            throw new InvalidOperationException("No safe exercises available for your injury profile.");

        var systemPrompt = SystemPromptFactory.Build(user.TonePersona, safeExercises);

        // Token-optimized user message with profile context
        var focusPart = string.IsNullOrWhiteSpace(request.WorkoutFocus)
            ? "full-body" : request.WorkoutFocus;
        var levelStr = request.ExperienceLevel.ToString().ToLowerInvariant();
        var userMessage = $"Workout: {focusPart} | Level: {levelStr}";

        if (!string.IsNullOrEmpty(user.Goal))
            userMessage += $" | Goal: {user.Goal}";
        if (!string.IsNullOrEmpty(user.EquipmentJson))
            userMessage += $" | Equipment: {user.EquipmentJson}";
        if (!string.IsNullOrEmpty(user.Injuries))
            userMessage += $" | Restrictions: {user.Injuries}";

        // Resolve provider and call LLM with fallback support for free models
        var provider = llmProviderFactory.GetProvider(providerName);
        var modelsToTry = new List<string> { preferredModel };

        if (providerName == "openrouter" || providerName == "groq")
        {
            var availableOnline = await provider.GetAvailableModelsAsync(apiKey, ct);
            var availableOnlineList = availableOnline.ToList();

            var otherFreeModels = LlmModelCatalog.GetByProvider(providerName)
                .Where(m => m.IsFree && m.ModelId != preferredModel)
                .Select(m => m.ModelId)
                .Where(id => availableOnlineList.Count == 0 || availableOnlineList.Contains(id));

            modelsToTry.AddRange(otherFreeModels);
        }

        string rawJson = string.Empty;
        Exception? lastException = null;

        foreach (var modelToTry in modelsToTry)
        {
            try
            {
                rawJson = await provider.ChatCompletionAsync(apiKey, modelToTry, systemPrompt, userMessage, forceJson: true, ct: ct);
                break;
            }
            catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("TooManyRequests") || ex.Message.Contains("404"))
            {
                lastException = ex;
                continue;
            }
        }

        if (string.IsNullOrEmpty(rawJson))
            throw new InvalidOperationException(
                $"Failed to generate workout after trying {modelsToTry.Count} model(s). Last error: {lastException?.Message}",
                lastException);

        // Safety Gate: sanitize hallucinated IDs and clamp weights
        var safeJson = SafetyGate.Validate(rawJson, safeExercises, request.ExperienceLevel);

        // === WARM-UP PREPEND (Task 1C) ===
        safeJson = PrependWarmUp(safeJson, allExercises, user.Injuries);

        // Cache for 2 hours
        await cache.SetAsync(cacheKey, safeJson, CacheTtl, ct);

        return new StartWorkoutResponse(safeJson);
    }

    /// <summary>
    /// Deterministically prepends 2-3 warm-up components to the LLM-generated payload.
    /// Warm-ups are NEVER LLM-generated — this is fully deterministic.
    /// Knee contraindications exclude Bodyweight Air Squats from warm-up.
    /// </summary>
    private static string PrependWarmUp(string mainJson, List<Domain.Entities.Exercise> allExercises, string? injuries)
    {
        try
        {
            var warmups = allExercises
                .Where(e => string.Equals(e.Category, "Warmup", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If user has knee issues, exclude Bodyweight Air Squats
            bool hasKneeInjury = HasInjuryKeyword(injuries, ["knee", "acl", "meniscus", "patella", "bad knees"]);
            if (hasKneeInjury)
                warmups = warmups.Where(e => e.Name != "Bodyweight Air Squats").ToList();

            // Select 2-3 warm-ups: prefer cardio first, then bodyweight
            var selected = warmups
                .OrderBy(e => e.Equipment == "cardio" ? 0 : 1)
                .Take(3)
                .ToList();

            if (selected.Count == 0)
                return mainJson;

            var doc = JsonNode.Parse(mainJson)!;
            var components = doc["components"]?.AsArray() ?? new JsonArray();

            // Build warm-up components and prepend them (after the tone_card)
            // tone_card stays at index 0, warm-ups go at index 1..N
            var toneCard = components.Count > 0 ? components[0]?.DeepClone() : null;
            var newComponents = new JsonArray();

            if (toneCard is not null)
                newComponents.Add(toneCard);

            foreach (var wu in selected)
            {
                newComponents.Add(JsonNode.Parse($$"""
                {
                    "type": "warmup_card",
                    "phase": "warmup",
                    "payload": {
                        "exercise_id": "{{wu.Id}}",
                        "exercise_name": "{{wu.Name}}",
                        "target_muscle": "{{wu.TargetMuscle}}",
                        "equipment": "{{wu.Equipment}}",
                        "duration_seconds": 300,
                        "notes": "Light warm-up to prepare your body. Keep it easy."
                    }
                }
                """)!);
            }

            // Add the original set_tracker components (skip tone_card which we already added)
            for (int i = 1; i < components.Count; i++)
            {
                var component = components[i]?.DeepClone();
                if (component is not null)
                    newComponents.Add(component);
            }

            doc["components"] = newComponents;
            return doc.ToJsonString();
        }
        catch
        {
            // Never break the workout if warm-up prepend fails
            return mainJson;
        }
    }

    private static bool HasInjuryKeyword(string? injuries, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(injuries)) return false;
        var lower = injuries.ToLowerInvariant();
        return keywords.Any(k => lower.Contains(k));
    }
}
