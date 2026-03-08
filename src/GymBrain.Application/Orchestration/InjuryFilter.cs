using System.Text.Json;
using GymBrain.Domain.Entities;

namespace GymBrain.Application.Orchestration;

/// <summary>
/// Deterministic pre-filter that removes contraindicated exercises from the
/// LLM catalog BEFORE the model ever sees them.
/// This prevents the LLM from recommending exercises that could harm a user
/// with stated injuries — regardless of how the prompt is phrased.
/// </summary>
public static class InjuryFilter
{
    private static readonly Lazy<JsonDocument> Map = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "ContraindicationMap.json");
        if (!File.Exists(path))
        {
            // Fallback: try relative to assembly location (for tests)
            path = Path.Combine(
                Path.GetDirectoryName(typeof(InjuryFilter).Assembly.Location) ?? "",
                "Data", "ContraindicationMap.json");
        }
        var json = File.ReadAllText(path);
        return JsonDocument.Parse(json);
    });

    /// <summary>
    /// Filters the catalog to remove exercises contraindicated by the user's
    /// stated injuries. If no injuries are specified, the full catalog is returned.
    /// Warm-up exercises (Category="Warmup") are always excluded from the main LLM catalog.
    /// </summary>
    public static IReadOnlyList<Exercise> Filter(IReadOnlyList<Exercise> allExercises, string? injuries)
    {
        // Always strip warm-up exercises from the LLM catalog
        var mainExercises = allExercises
            .Where(e => !string.Equals(e.Category, "Warmup", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (string.IsNullOrWhiteSpace(injuries))
            return mainExercises;

        var excludedIds = GetExcludedIds(injuries);
        if (excludedIds.Count == 0)
            return mainExercises;

        return mainExercises
            .Where(e => !excludedIds.Contains(e.Id.ToString(), StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Returns the set of exercise IDs to exclude based on injury keyword matching.
    /// Uses case-insensitive substring matching against the injury text.
    /// </summary>
    public static HashSet<string> GetExcludedIds(string? injuries)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(injuries))
            return result;

        var injuryLower = injuries.ToLowerInvariant();
        var root = Map.Value.RootElement;

        if (!root.TryGetProperty("contraindications", out var groups))
            return result;

        foreach (var group in groups.EnumerateArray())
        {
            if (!group.TryGetProperty("keywords", out var keywords)) continue;
            if (!group.TryGetProperty("excluded_exercise_ids", out var ids)) continue;

            var matched = keywords.EnumerateArray()
                .Any(kw => injuryLower.Contains(kw.GetString()!.ToLowerInvariant()));

            if (!matched) continue;

            foreach (var id in ids.EnumerateArray())
            {
                var idStr = id.GetString();
                if (!string.IsNullOrEmpty(idStr))
                    result.Add(idStr);
            }
        }

        return result;
    }
}
