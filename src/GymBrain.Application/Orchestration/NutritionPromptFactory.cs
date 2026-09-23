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

        // The app currently requests one day. Keep that response focused on the
        // recipe fields it actually displays; optional planning metadata made
        // reasoning models exhaust their output budget before closing the JSON.
        if (durationDays == 1)
        {
            return $$"""
                Return one complete JSON object containing a one-day meal plan. No markdown or extra text.
                Diet: {{diet}}. Goal: {{goal}}. Target: {{calories}} kcal for the day.
                Coach tone: {{tonePersona}}. Location: {{locationLine}}.
                Monthly budget: {{budgetLine}}. Cooking resources: {{resources}}.

                Use exactly this compact shape:
                {
                  "message_from_coach": "one short sentence",
                  "days": [{
                    "day_number": 1,
                    "meals": [{
                      "type": "Breakfast, Lunch, or Dinner",
                      "name": "recipe name",
                      "calories": 600,
                      "protein_g": 25,
                      "carbs_g": 70,
                      "fat_g": 20,
                      "description": "one short sentence",
                      "servings": 1,
                      "prep_minutes": 10,
                      "cook_minutes": 15,
                      "ingredients": [{ "name": "ingredient", "quantity": "measured amount" }],
                      "steps": ["actionable cooking step"]
                    }]
                  }]
                }
                Include exactly three complete meals with measured ingredients and ordered cooking steps.
                Keep descriptions and steps concise so the JSON finishes within the output limit.
                Respect the stated diet and resources. Calories and macros are estimates per serving.
                Never claim medical suitability. Return valid JSON only.
                """;
        }

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
                      "description": "string",
                      "servings": "integer",
                      "prep_minutes": "integer",
                      "cook_minutes": "integer",
                      "ingredients": [{ "name": "string", "quantity": "string" }],
                      "steps": ["string"]
                    }
                  ]
                }
              ]
            }

            RULES:
            - days array MUST have exactly {{durationDays}} entries
            - Each day must contain 3 to 5 meals
            - Every meal needs ingredient quantities and ordered, actionable cooking steps
            - State servings and prep/cook time; calories and macros are estimates per serving
            - Include appropriate cooking and storage guidance; never claim medical suitability
            - total_calories should stay within +/- 100 kcal of target
            - Use geographically sensible, budget-aware ingredients and meal choices
            - Respect the available resources when selecting recipes and prep complexity
            - shopping_notes should help the user buy once and reuse ingredients efficiently
            - reminders should be practical and non-spammy
            - Prioritize complete, valid JSON and all meal recipes over optional narrative. Keep notes and descriptions short so the answer fits the output limit.
            - Output raw JSON only. No markdown fences, no backticks.
            """;
    }
}
