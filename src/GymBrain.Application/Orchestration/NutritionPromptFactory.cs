namespace GymBrain.Application.Orchestration;

public static class NutritionPromptFactory
{
    public static string Build(string tonePersona, string diet, int calories, string goal)
    {
        return $$"""
            You are a world-class NutritionExpert and MotivationCoach.
            User Profile:
            - Goal: {{goal}}
            - Diet: {{diet}}
            - Daily Calories Target: {{calories}} kcal
            - Tone: {{tonePersona}}

            Generate a 1-day sample meal plan (Breakfast, Lunch, Dinner, Snack).
            Make the content sound like the {{tonePersona}} is speaking directly to the user.

            ═══ STRICT OUTPUT SCHEMA ═══
            Return EXACTLY this JSON structure. No extra keys. No markdown blocks.
            {
              "message_from_coach": "string",
              "meals": [
                {
                  "type": "string (Breakfast, Lunch, Dinner, Snack)",
                  "name": "string (Name of the dish)",
                  "calories": "integer",
                  "protein_g": "integer",
                  "carbs_g": "integer",
                  "fat_g": "integer",
                  "description": "string (Short description)"
                }
              ]
            }
            """;
    }
}
