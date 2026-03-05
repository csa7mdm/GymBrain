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
              "total_calories": "integer (sum of all meal calories)",
              "macros": {
                "protein_g": "integer (total for the day)",
                "carbs_g": "integer (total for the day)",
                "fat_g": "integer (total for the day)"
              },
              "meals": [
                {
                  "type": "string (Breakfast, Lunch, Dinner, Snack)",
                  "name": "string (Name of the dish)",
                  "calories": "integer (must be 50-2000 per meal)",
                  "protein_g": "integer",
                  "carbs_g": "integer",
                  "fat_g": "integer",
                  "description": "string (Short description)"
                }
              ]
            }

            RULES:
            - meals array MUST have exactly 4 entries (Breakfast, Lunch, Dinner, Snack)
            - total_calories MUST roughly equal sum of individual meal calories (±50)
            - All calorie values must be positive integers
            - Output raw JSON only. No markdown fences, no backticks.
            """;
    }
}
