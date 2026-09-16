using System.Text.Json;
using System.Text.Json.Nodes;
using GymBrain.Domain.Entities;
using GymBrain.Domain.Enums;

namespace GymBrain.Application.Orchestration;

public static class SafetyGate
{
    public static string Validate(string rawJson, IReadOnlyList<Exercise> validExercises, ExperienceLevel level)
    {
        try
        {
            var start = rawJson.IndexOf('{'); var end = rawJson.LastIndexOf('}');
            if (start < 0 || end <= start) throw new JsonException();
            var root = JsonNode.Parse(rawJson[start..(end + 1)]) as JsonObject ?? throw new JsonException();
            var catalog = validExercises.ToDictionary(e => e.Id.ToString(), StringComparer.OrdinalIgnoreCase);
            if (catalog.Count == 0) throw new JsonException();
            var maxWeight = level switch { ExperienceLevel.Advanced or ExperienceLevel.Athlete => 200, ExperienceLevel.Intermediate => 100, _ => 40 };
            var components = new JsonArray();
            var count = 0;
            if (root["components"] is JsonArray input)
            {
                if (input.Count > 21) throw new JsonException();
                foreach (var component in input)
                {
                    var type = component?["type"]?.GetValue<string>();
                    var payload = component?["payload"] as JsonObject ?? throw new JsonException();
                    if (type == "tone_card")
                    {
                        if (components.Count != 0) throw new JsonException();
                        components.Add(new JsonObject { ["type"] = "tone_card", ["payload"] = new JsonObject {
                            ["message"] = Text(payload, "message", 1000), ["persona"] = Text(payload, "persona", 100) } });
                    }
                    else if (type == "set_tracker")
                    {
                        components.Add(new JsonObject { ["type"] = type, ["payload"] = ExercisePayload(payload, catalog, maxWeight) });
                        count++;
                    }
                    else throw new JsonException();
                }
            }
            else if (root.ContainsKey("exercise_id"))
            {
                components.Add(new JsonObject { ["type"] = "set_tracker", ["payload"] = ExercisePayload(root, catalog, maxWeight) });
                count++;
            }
            if (count == 0) throw new JsonException();
            return new JsonObject { ["screen_id"] = "workout_today", ["components"] = components }
                .ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            throw new InvalidOperationException("The generated workout did not pass validation. Please try again.", ex);
        }
    }

    private static JsonObject ExercisePayload(JsonObject payload, Dictionary<string, Exercise> catalog, double maxWeight)
    {
        var id = payload["exercise_id"]?.GetValue<string>() ?? "";
        if (!catalog.TryGetValue(id, out var exercise)) throw new JsonException();
        // Identity and equipment always come from the filtered catalog, never model text.
        return new JsonObject {
            ["exercise_id"] = exercise.Id.ToString(), ["exercise_name"] = exercise.Name,
            ["target_muscle"] = exercise.TargetMuscle, ["equipment"] = exercise.Equipment,
            ["sets"] = (int)Number(payload, "sets", 3, 1, 10),
            ["reps"] = (int)Number(payload, "reps", 10, 1, 30),
            ["weight_kg"] = Number(payload, "weight_kg", 0, 0, maxWeight),
            ["rest_seconds"] = (int)Number(payload, "rest_seconds", 90, 30, 300),
            ["coach_tip"] = Text(payload, "coach_tip", 500)
        };
    }
    private static double Number(JsonObject payload, string key, double fallback, double min, double max)
    {
        if (!payload.ContainsKey(key)) return fallback;
        var value = payload[key]?.GetValue<double>() ?? throw new JsonException();
        if (!double.IsFinite(value)) throw new JsonException();
        return Math.Clamp(value, min, max);
    }
    private static string Text(JsonObject payload, string key, int limit)
    {
        var text = payload[key]?.GetValue<string>() ?? "";
        return text.Length <= limit ? text : text[..limit];
    }
}
