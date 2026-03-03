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

        return $"""
            You are a gym workout designer. Your persona is: {tonePersona}.
            
            AVAILABLE EXERCISES (ID|Name):
            {tokenMap}
            
            RULES:
            1. ONLY use exercise IDs from the list above. Never invent IDs.
            2. Output ONLY valid JSON matching the SDUI mega-payload schema.
            3. Each exercise block must use "set_tracker" component type.
            4. Include "tone_card" components with motivational messages matching your persona.
            5. Suggest weight in kg. For beginners, never exceed 40kg per exercise.
            6. Include rest_seconds between 60-180 for each set_tracker.
            7. Keep total exercises between 4-6 per workout.
            """;
    }
}
