namespace GymBrain.Application.Common;

/// <summary>
/// Static catalog of available LLM models across all supported providers.
/// Models are sorted by suitability for GymBrain's use case:
///   1. JSON mode support (critical for SDUI mega-payloads)
///   2. Instruction following quality
///   3. Cost (free preferred)
///   4. Speed
/// </summary>
public static class LlmModelCatalog
{
    public static readonly IReadOnlyList<LlmModelInfo> AllModels = new List<LlmModelInfo>
    {
        // ── OpenAI ──────────────────────────────
        new("openai", "gpt-4o-mini", "GPT-4o Mini", 
            "Best JSON mode · Fast · $0.15/1M in", IsFree: false, SuitabilityRank: 1),
        new("openai", "gpt-4o", "GPT-4o", 
            "Top quality · JSON mode · $2.50/1M in", IsFree: false, SuitabilityRank: 3),

        // ── Groq (ALL FREE with rate limits) ────
        new("groq", "llama-3.3-70b-versatile", "Llama 3.3 70B", 
            "Best free model · JSON mode · 128k context", IsFree: true, SuitabilityRank: 2),
        new("groq", "llama-3.1-8b-instant", "Llama 3.1 8B Instant", 
            "Fastest free · Good for simple workouts", IsFree: true, SuitabilityRank: 5),
        new("groq", "gemma2-9b-it", "Gemma 2 9B", 
            "Google model · Good instruction following", IsFree: true, SuitabilityRank: 6),
        new("groq", "mixtral-8x7b-32768", "Mixtral 8x7B", 
            "Strong reasoning · 32k context", IsFree: true, SuitabilityRank: 4),

        // ── OpenRouter (FREE models — 2026 Updated) ─
        new("openrouter", "meta-llama/llama-3.3-70b-instruct:free", "Llama 3.3 70B (Free)", 
            "Best free logic · Highly rate limited", IsFree: true, SuitabilityRank: 3),
        new("openrouter", "google/gemma-3-27b-it:free", "Gemma 3 27B (Free)", 
            "Latest Google Model · Excellent JSON", IsFree: true, SuitabilityRank: 4),
        new("openrouter", "deepseek/deepseek-chat-v3-0324:free", "DeepSeek V3 (Free)", 
            "Fast · Strong instruction following", IsFree: true, SuitabilityRank: 5),
        new("openrouter", "qwen/qwen3-next-80b-a3b-instruct:free", "Qwen 3 Next (Free)", 
            "Powerful multilingual logic", IsFree: true, SuitabilityRank: 6),
        new("openrouter", "mistralai/mistral-small-3.1-24b-instruct:free", "Mistral Small (Free)", 
            "Reliable and fast", IsFree: true, SuitabilityRank: 8),

        // ── Anthropic ───────────────────────────
        new("anthropic", "claude-sonnet-4-20250514", "Claude Sonnet 4", 
            "Excellent reasoning · $3/1M in", IsFree: false, SuitabilityRank: 2),
        new("anthropic", "claude-3-5-haiku-20241022", "Claude 3.5 Haiku", 
            "Fast + cheap · $0.80/1M in", IsFree: false, SuitabilityRank: 4),
    };

    public static IEnumerable<LlmModelInfo> GetByProvider(string provider) =>
        AllModels
            .Where(m => m.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.SuitabilityRank);

    public static IEnumerable<string> SupportedProviders =>
        AllModels.Select(m => m.Provider).Distinct();

    public static string GetDefaultModel(string provider) =>
        GetByProvider(provider).FirstOrDefault()?.ModelId
        ?? throw new ArgumentException($"Unknown provider: {provider}");

    public static bool IsValidModel(string provider, string modelId) =>
        AllModels.Any(m =>
            m.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) &&
            m.ModelId.Equals(modelId, StringComparison.OrdinalIgnoreCase));
}

public record LlmModelInfo(
    string Provider,
    string ModelId,
    string DisplayName,
    string Description,
    bool IsFree,
    int SuitabilityRank);
