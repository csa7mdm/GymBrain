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
    ILlmProvider llmProvider,
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
        var userMessage = string.IsNullOrWhiteSpace(request.WorkoutFocus)
            ? "Generate a full-body workout for today."
            : $"Generate a workout focused on: {request.WorkoutFocus}";

        // Call LLM
        var rawJson = await llmProvider.ChatCompletionAsync(apiKey, systemPrompt, userMessage, ct);

        // Safety Gate: sanitize hallucinated IDs and clamp weights
        var safeJson = SafetyGate.Validate(rawJson, exercises, request.ExperienceLevel);

        // Cache for 2 hours (prevents redundant LLM calls on app reload)
        await cache.SetAsync(cacheKey, safeJson, CacheTtl, ct);

        return new StartWorkoutResponse(safeJson);
    }
}
