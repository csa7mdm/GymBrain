using GymBrain.Domain.Entities;

namespace GymBrain.Application.Orchestration;

/// <summary>
/// Builds token-compressed system prompts for the LLM.
/// KEY DESIGN: Exercises are compressed to "ID|Name" format to minimize token usage.
/// Full descriptions, muscle targets, and categories are NOT sent — only the
/// minimum data needed for the LLM to generate a valid SDUI mega-payload.
/// This keeps us under the $0.05/session COGS ceiling.
/// </summary>
public static class SystemPromptFactory
{
    /// <summary>
    /// Builds the compressed exercise token map: "ID|Name" per line.
    /// This is the ONLY exercise data sent to the LLM.
    /// </summary>
    public static string BuildTokenMap(IReadOnlyList<Exercise> exercises)
    {
        return string.Join('\n', exercises.Select(e => $"{e.Id}|{e.Name}"));
    }

    /// <summary>
    /// Constructs the full system prompt with tone persona and available exercises.
    /// Prompt is tightly scoped to minimize token consumption.
    /// </summary>
    public static string Build(string tonePersona, IReadOnlyList<Exercise> exercises)
    {
        var tokenMap = BuildTokenMap(exercises);

        var jsonExample = """
            {
              "screen_id": "workout_today",
              "components": [
                {
                  "type": "tone_card",
                  "payload": {
                    "message": "Your motivational message here",
                    "persona": "Coach"
                  }
                },
                {
                  "type": "set_tracker",
                  "payload": {
                    "exercise_id": "<ID from the list above>",
                    "exercise_name": "<Name from the list above>",
                    "target_muscle": "<primary muscle group>",
                    "sets": 3,
                    "reps": 10,
                    "weight_kg": 20,
                    "rest_seconds": 90
                  }
                }
              ]
            }
            """;

        return $"""
            You are a gym workout designer. Your persona is: {tonePersona}.
            
            AVAILABLE EXERCISES (ID|Name):
            {tokenMap}
            
            RULES:
            1. ONLY use exercise IDs from the list above. Never invent IDs.
            2. Output ONLY valid JSON — no markdown, no backticks, no explanation.
            3. Suggest weight in kg. For beginners never exceed 40kg.
            4. Keep total exercises between 4-6 per workout.
            5. rest_seconds must be between 60-180.

            YOU MUST return EXACTLY this JSON structure (no extra keys, no renaming):
            {jsonExample}

            Return 1 tone_card at the start, then 4-6 set_tracker components.
            Use persona "{tonePersona}" in the tone_card payload.
            """;
    }
}
