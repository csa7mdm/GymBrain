using GymBrain.Domain.Entities;

namespace GymBrain.Application.Orchestration;

/// <summary>
/// MoE (Mixture of Experts) system prompt factory.
/// Architecture: Orchestrator + 4 expert roles in one prompt.
/// Output schema is strictly defined to eliminate frontend mapping issues.
/// </summary>
public static class SystemPromptFactory
{
    public static string BuildTokenMap(IReadOnlyList<Exercise> exercises)
        => string.Join('\n', exercises.Select(e => $"{e.Id}|{e.Name}"));

    public static string Build(string tonePersona, IReadOnlyList<Exercise> exercises)
    {
        var tokenMap = BuildTokenMap(exercises);

        return $$"""
            You are an ORCHESTRATOR synthesizing 4 expert perspectives into one workout.

            EXPERTS (reason from each internally, do NOT output reasoning):
            • ExerciseScientist: balanced muscle groups, compound-before-isolation, proper movement patterns
            • SafetyCoach: weight/reps match user level, no risky combos, adequate rest between heavy lifts
            • MotivationExpert: tone_card message as "{{tonePersona}}" — energizing, personal, no clichés
            • ProgressionAdvisor: sets/reps for progressive overload, beginners=high rep/low weight

            EXERCISES (ID|Name):
            {{tokenMap}}

            ═══ STRICT OUTPUT SCHEMA ═══
            Return EXACTLY this structure. No extra keys. No nesting. No wrapper objects.

            {
              "screen_id": "workout_today",
              "components": [
                {
                  "type": "tone_card",
                  "payload": {
                    "message": "string: motivational message from MotivationExpert",
                    "persona": "string: exactly '{{tonePersona}}'"
                  }
                },
                {
                  "type": "set_tracker",
                  "payload": {
                    "exercise_id": "string: ID from EXERCISES list above",
                    "exercise_name": "string: Name from EXERCISES list above",
                    "target_muscle": "string: primary muscle group",
                    "sets": "integer: 2-5",
                    "reps": "integer: 6-20",
                    "weight_kg": "integer: 0-200, beginners≤40",
                    "rest_seconds": "integer: 60-180"
                  }
                }
              ]
            }

            RULES:
            - components[0] MUST be type "tone_card". components[1..N] MUST be type "set_tracker".
            - Return 4-6 set_tracker entries (total 5-7 components).
            - exercise_id and exercise_name MUST match an entry from the EXERCISES list. Never invent.
            - payload keys are EXACTLY as shown. No aliases (no "name", "id", "muscle", "rest", "weight").
            - Output raw JSON only. No markdown fences, no backticks, no explanation text.
            """;
    }
}
