namespace GymBrain.Application.Orchestration;

public static class NutritionPromptFactory
{
    public static string Build(
        string tonePersona,
        string diet,
        int calories,
        string goal,
        int durationDays,
        decimal? monthlyBudget,
        string? currencyCode,
        string? country,
        string? city,
        string[]? availableResources,
        string? reminderTime)
    {
        var resources = availableResources is { Length: > 0 }
            ? string.Join(", ", availableResources)
            : "No special resources provided";
        var budgetLine = monthlyBudget.HasValue
            ? $"{monthlyBudget.Value:0.##} {currencyCode ?? "local currency"} per month"
            : "Not specified";
        var locationLine = string.Join(", ", new[] { city, country }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (string.IsNullOrWhiteSpace(locationLine))
            locationLine = "Not specified";

        return $$"""
            You are a world-class NutritionExpert, MealPrepPlanner, and BudgetAwareCoach.
            User Profile:
            - Goal: {{goal}}
            - Diet: {{diet}}
            - Daily Calories Target: {{calories}} kcal
            - Tone: {{tonePersona}}
            - Duration: {{durationDays}} days
            - Monthly Budget: {{budgetLine}}
            - Location: {{locationLine}}
            - Available Resources: {{resources}}
            - Preferred Reminder Time: {{reminderTime ?? "08:00"}}

            Generate a structured meal plan for the full requested duration.
            The plan must reflect local availability and pricing assumptions for the user's location.
            If exact local pricing is unknown, estimate conservatively and say so in budget_notes.
            Keep meals realistic for the stated resources and equipment.
            Make the content sound like the {{tonePersona}} is speaking directly to the user.

            ═══ STRICT OUTPUT SCHEMA ═══
            Return EXACTLY this JSON structure. No extra keys. No markdown blocks.
            {
              "message_from_coach": "string",
              "plan_name": "string",
              "duration_days": "integer",
              "location": {
                "country": "string",
                "city": "string",
                "currency_code": "string"
              },
              "budget": {
                "monthly_budget": "number",
                "estimated_total_cost": "number",
                "estimated_daily_cost": "number",
                "budget_notes": "string"
              },
              "available_resources": ["string"],
              "shopping_notes": ["string"],
              "reminders": [
                {
                  "title": "string",
                  "time_of_day": "HH:mm",
                  "days": ["string"]
                }
              ],
              "days": [
                {
                  "day_number": "integer",
                  "title": "string",
                  "focus": "string",
                  "total_calories": "integer",
                  "estimated_cost": "number",
                  "meals": [
                    {
                      "type": "string",
                      "name": "string",
                      "calories": "integer",
                      "protein_g": "integer",
                      "carbs_g": "integer",
                      "fat_g": "integer",
                      "description": "string"
                    }
                  ]
                }
              ]
            }

            RULES:
            - days array MUST have exactly {{durationDays}} entries
            - Each day must contain 3 to 5 meals
            - total_calories should stay within +/- 100 kcal of target
            - Use geographically sensible, budget-aware ingredients and meal choices
            - Respect the available resources when selecting recipes and prep complexity
            - shopping_notes should help the user buy once and reuse ingredients efficiently
            - reminders should be practical and non-spammy
            - Output raw JSON only. No markdown fences, no backticks.
            """;
    }
}
