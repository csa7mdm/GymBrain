using System.Text.Json;
using GymBrain.Domain.Entities;

namespace GymBrain.Application.Orchestration;

public static class EquipmentFilter
{
    public static IReadOnlyList<Exercise> Filter(IReadOnlyList<Exercise> exercises, string? equipmentJson)
    {
        var available = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bodyweight" };
        try
        {
            foreach (var item in JsonSerializer.Deserialize<string[]>(equipmentJson ?? "[]") ?? [])
                available.Add((item ?? "").Trim().ToLowerInvariant() switch {
                    "dumbbells" => "dumbbell", "cables" => "cable", "machines" => "machine",
                    "kettlebells" => "kettlebell", "bands" => "band", var value => value
                });
        }
        catch (JsonException) { /* No equipment assumptions for malformed legacy profiles. */ }
        return exercises.Where(e => available.Contains(RequiredEquipment(e))).ToList();
    }

    private static string RequiredEquipment(Exercise exercise) => exercise.Name switch
    {
        // These catalog entries use bodyweight resistance but still require apparatus.
        "Pull-Up" or "Chin-Up" or "Hanging Leg Raise" => "pull-up bar",
        "Back Extension" => "machine",
        "Dips" => "parallel bars",
        "Ab Wheel Rollout" => "ab wheel",
        "Box Jump" => "box",
        "Medicine Ball Slam" => "medicine ball",
        _ => exercise.Equipment
    };
}
