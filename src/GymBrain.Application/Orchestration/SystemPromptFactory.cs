using GymBrain.Domain.Entities;

namespace GymBrain.Application.Orchestration;

/// <summary>
/// Builds a compact request using only exercises the user can currently do.
/// </summary>
public static class SystemPromptFactory
{
    public static string BuildTokenMap(IReadOnlyList<Exercise> exercises)
        => string.Join('\n', exercises.Select(e => $"{e.Id}|{e.Name}"));

    public static string Build(string tonePersona, IReadOnlyList<Exercise> exercises, int workoutsCompleted)
    {
        var tokenMap = BuildTokenMap(exercises);
        
        int minExercises = workoutsCompleted >= 10 ? 6 : workoutsCompleted >= 3 ? 5 : 4;
        int maxExercises = workoutsCompleted >= 10 ? 8 : workoutsCompleted >= 3 ? 7 : 6;

        return $$"""
            Create a balanced workout using only the safe EXERCISES listed below (ID|Name).
            Return one complete JSON object, no markdown or extra text:
            {"exercises":[{"exercise_id":"ID from list","sets":3,"reps":10,"weight_kg":0,"rest_seconds":90,"coach_tip":"brief form cue"}]}

            Return {{minExercises}} to {{maxExercises}} distinct exercises. Every exercise_id MUST match an entry from the EXERCISES list. Never invent an ID.
            Use compound movements before isolation, conservative weights for the user's level, and enough rest.
            For endurance, favor moderate-to-high reps and manageable loads. Use zero weight for bodyweight exercises.
            Keep each coach_tip short, practical, and in the tone of {{tonePersona}}. Avoid medical claims.
            Keep the answer concise and finish the JSON before the output limit.

            EXERCISES (ID|Name):
            {{tokenMap}}
            """;
    }
}
