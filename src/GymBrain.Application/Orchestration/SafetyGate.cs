using System.Text.Json;
using GymBrain.Domain.Entities;
using GymBrain.Domain.Enums;

namespace GymBrain.Application.Orchestration;

/// <summary>
/// C# deterministic rule engine that validates and sanitizes raw LLM JSON output.
/// Enforces:
/// 1. Exercise IDs must exist in the seeded catalog (anti-hallucination).
/// 2. Weight caps per ExperienceLevel (Beginner ≤ 40kg).
/// 3. Structural JSON integrity.
/// </summary>
public static class SafetyGate
{
    private const double BeginnerMaxWeightKg = 40.0;
    private const double IntermediateMaxWeightKg = 100.0;
    private const double AdvancedMaxWeightKg = 200.0;

    /// <summary>
    /// Validates and clamps the raw LLM JSON mega-payload.
    /// </summary>
    public static string Validate(
        string rawJson,
        IReadOnlyList<Exercise> validExercises,
        ExperienceLevel level)
    {
        var validIds = validExercises.Select(e => e.Id.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var fallbackId = validExercises.First().Id.ToString();
        var maxWeight = GetMaxWeight(level);

        using var doc = JsonDocument.Parse(rawJson);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        ProcessElement(doc.RootElement, writer, validIds, fallbackId, maxWeight);

        writer.Flush();
        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }

    private static double GetMaxWeight(ExperienceLevel level) => level switch
    {
        ExperienceLevel.Beginner => BeginnerMaxWeightKg,
        ExperienceLevel.Intermediate => IntermediateMaxWeightKg,
        ExperienceLevel.Advanced => AdvancedMaxWeightKg,
        _ => BeginnerMaxWeightKg
    };

    private static void ProcessElement(
        JsonElement element,
        Utf8JsonWriter writer,
        HashSet<string> validIds,
        string fallbackId,
        double maxWeight)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var prop in element.EnumerateObject())
                {
                    writer.WritePropertyName(prop.Name);

                    // Clamp exercise_id to valid catalog IDs
                    if (prop.Name.Equals("exercise_id", StringComparison.OrdinalIgnoreCase)
                        && prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var id = prop.Value.GetString() ?? "";
                        writer.WriteStringValue(validIds.Contains(id) ? id : fallbackId);
                    }
                    // Clamp weight_kg to experience level cap
                    else if (prop.Name.Equals("weight_kg", StringComparison.OrdinalIgnoreCase)
                             && prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        var weight = prop.Value.GetDouble();
                        writer.WriteNumberValue(Math.Min(Math.Max(weight, 0), maxWeight));
                    }
                    // Clamp reps to 1-30 range
                    else if (prop.Name.Equals("reps", StringComparison.OrdinalIgnoreCase)
                             && prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        var reps = (int)prop.Value.GetDouble();
                        writer.WriteNumberValue(Math.Clamp(reps, 1, 30));
                    }
                    // Clamp rest_seconds to 30-300 range
                    else if (prop.Name.Equals("rest_seconds", StringComparison.OrdinalIgnoreCase)
                             && prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        var rest = (int)prop.Value.GetDouble();
                        writer.WriteNumberValue(Math.Clamp(rest, 30, 300));
                    }
                    // Clamp sets to 1-10 range
                    else if (prop.Name.Equals("sets", StringComparison.OrdinalIgnoreCase)
                             && prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        var sets = (int)prop.Value.GetDouble();
                        writer.WriteNumberValue(Math.Clamp(sets, 1, 10));
                    }
                    else
                    {
                        ProcessElement(prop.Value, writer, validIds, fallbackId, maxWeight);
                    }
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    ProcessElement(item, writer, validIds, fallbackId, maxWeight);
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteNumberValue(element.GetDouble());
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;

            default:
                writer.WriteNullValue();
                break;
        }
    }
}
