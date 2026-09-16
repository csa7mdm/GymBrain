using FluentValidation;
using System.Text.Json;

namespace GymBrain.Application.Orchestration.Commands;

public sealed class SaveWorkoutCommandValidator : AbstractValidator<SaveWorkoutCommand>
{
    public SaveWorkoutCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).Must(id => id == null || id != Guid.Empty);
        RuleFor(x => x.PayloadJson).Cascade(CascadeMode.Stop).NotEmpty()
            .MaximumLength(65536).Must((command, json) => ValidPayload(json, command.SessionId.HasValue)).WithMessage("Invalid workout completion data.");
    }

    private static bool ValidPayload(string json, bool requireCompletion)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            // Legacy generated plans remain readable/savable during frontend rollout.
            if (root.ValueKind == JsonValueKind.Array) return !requireCompletion;
            if (root.ValueKind != JsonValueKind.Object) return false;
            if (!root.TryGetProperty("schemaVersion", out var version)) return !requireCompletion;
            if (!version.TryGetInt32(out var v) || v != 1) return false;
            if (!root.TryGetProperty("focus", out var focus) || focus.ValueKind != JsonValueKind.String || focus.GetString()!.Length > 200) return false;
            if (!root.TryGetProperty("exercises", out var exercises) || exercises.ValueKind != JsonValueKind.Array || exercises.GetArrayLength() is < 1 or > 50) return false;
            var completed = 0;
            foreach (var exercise in exercises.EnumerateArray())
            {
                if (!exercise.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(name.GetString()) || name.GetString()!.Length > 200) return false;
                if (!exercise.TryGetProperty("sets", out var sets) || sets.ValueKind != JsonValueKind.Array || sets.GetArrayLength() is < 1 or > 50) return false;
                foreach (var set in sets.EnumerateArray())
                {
                    if (!set.TryGetProperty("completed", out var done) || done.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) return false;
                    if (!set.TryGetProperty("reps", out var reps) || !reps.TryGetInt32(out var n) || n is < 0 or > 1000) return false;
                    if (!set.TryGetProperty("weightKg", out var weight) || !weight.TryGetDouble(out var kg) || !double.IsFinite(kg) || kg is < 0 or > 1000) return false;
                    if (done.GetBoolean()) completed++;
                }
            }
            return completed > 0;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException) { return false; }
    }
}
