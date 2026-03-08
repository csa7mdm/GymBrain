using GymBrain.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GymBrain.Application.Orchestration.Commands;

/// <summary>
/// Handles the "Machine Taken" substitute lookup.
/// 1. Loads SubstituteMap.json
/// 2. Filters alternatives by the user's injury contraindications
/// 3. Returns only safe, equipment-different alternatives
/// </summary>
public sealed class GetSubstituteQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSubstituteQuery, SubstituteResult>
{
    private static readonly Lazy<JsonDocument> SubMap = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "SubstituteMap.json");
        if (!File.Exists(path))
        {
            path = Path.Combine(
                Path.GetDirectoryName(typeof(GetSubstituteQueryHandler).Assembly.Location) ?? "",
                "Data", "SubstituteMap.json");
        }
        return JsonDocument.Parse(File.ReadAllText(path));
    });

    public async Task<SubstituteResult> Handle(GetSubstituteQuery request, CancellationToken ct)
    {
        var idStr = request.ExerciseId.ToString();

        // Load original exercise for metadata
        var original = await db.Exercises.FirstOrDefaultAsync(e => e.Id == request.ExerciseId, ct);
        var originalName = original?.Name ?? idStr;

        if (!SubMap.Value.RootElement.TryGetProperty("substitutes", out var subs))
        {
            return new SubstituteResult(idStr, originalName,
                Array.Empty<SubstituteOption>(),
                "Substitute map unavailable. Contact support.");
        }

        if (!subs.TryGetProperty(idStr, out var entry))
        {
            return new SubstituteResult(idStr, originalName,
                Array.Empty<SubstituteOption>(),
                "No substitutes mapped for this exercise. Consider skipping it.");
        }

        // Get contraindicated IDs for this user
        var excludedIds = InjuryFilter.GetExcludedIds(request.UserInjuries);

        var alternatives = new List<SubstituteOption>();
        if (entry.TryGetProperty("alternatives", out var alts))
        {
            foreach (var alt in alts.EnumerateArray())
            {
                var altId = alt.GetProperty("exercise_id").GetString() ?? "";
                if (excludedIds.Contains(altId)) continue; // skip contraindicated

                alternatives.Add(new SubstituteOption(
                    ExerciseId: altId,
                    Name: alt.GetProperty("name").GetString() ?? "",
                    Equipment: alt.GetProperty("equipment").GetString() ?? "",
                    Reason: alt.GetProperty("reason").GetString() ?? ""
                ));
            }
        }

        var message = alternatives.Count == 0
            ? "No safe substitutes available for your injury profile. Consider skipping this exercise."
            : null;

        return new SubstituteResult(idStr, originalName, alternatives, message);
    }
}
