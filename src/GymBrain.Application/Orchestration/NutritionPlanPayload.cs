using System.Text.Json;

namespace GymBrain.Application.Orchestration;

public static class NutritionPlanPayload
{
    public static string ValidateAndExtract(string response, int expectedDays = 1)
    {
        if (string.IsNullOrWhiteSpace(response) || response.Length > 500_000)
            throw Invalid();

        var start = response.IndexOf('{');
        var end = response.LastIndexOf('}');
        if (start < 0 || end <= start) throw Invalid();
        var json = response[start..(end + 1)];
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw Invalid();
            var plan = Child(root, "meal_plan") ?? Child(root, "plan") ?? root;
            var meals = Child(plan, "meals");
            if (meals is { ValueKind: JsonValueKind.Array })
            {
                if (expectedDays != 1) throw Invalid();
                ValidateMeals(meals.Value);
            }
            else if (Child(plan, "days") is { ValueKind: JsonValueKind.Array } days)
            {
                if (days.GetArrayLength() != expectedDays) throw Invalid();
                var seenDays = new HashSet<int>();
                var count = 0;
                foreach (var day in days.EnumerateArray())
                {
                    if (expectedDays > 1)
                    {
                        var number = Child(day, "day_number") ?? Child(day, "day");
                        if (number is not { ValueKind: JsonValueKind.Number } n || !n.TryGetInt32(out var dayNumber) ||
                            dayNumber < 1 || dayNumber > expectedDays || !seenDays.Add(dayNumber)) throw Invalid();
                    }
                    if (Child(day, "meals") is not { ValueKind: JsonValueKind.Array } dayMeals) throw Invalid();
                    count += ValidateMeals(dayMeals);
                }
                if (count == 0) throw Invalid();
            }
            else throw Invalid();
            return json;
        }
        catch (JsonException) { throw Invalid(); }
    }

    private static int ValidateMeals(JsonElement meals)
    {
        var count = 0;
        foreach (var meal in meals.EnumerateArray())
        {
            if (meal.ValueKind != JsonValueKind.Object ||
                !HasText(meal, "name", "title") ||
                Child(meal, "ingredients") is not { ValueKind: JsonValueKind.Array } ingredients ||
                ingredients.GetArrayLength() == 0 ||
                !HasSteps(meal)) throw Invalid();
            count++;
        }
        if (count == 0) throw Invalid();
        return count;
    }

    private static bool HasSteps(JsonElement meal)
    {
        foreach (var name in new[] { "steps", "instructions", "directions" })
        {
            var value = Child(meal, name);
            if (value is { ValueKind: JsonValueKind.Array } array && array.GetArrayLength() > 0) return true;
            if (value is { ValueKind: JsonValueKind.String } text && !string.IsNullOrWhiteSpace(text.GetString())) return true;
        }
        return false;
    }

    private static bool HasText(JsonElement parent, params string[] names) => names.Any(name =>
        Child(parent, name) is { ValueKind: JsonValueKind.String } value && !string.IsNullOrWhiteSpace(value.GetString()));

    private static JsonElement? Child(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) ? value : null;

    private static ArgumentException Invalid() => new("The model returned an incomplete meal plan. Try another model in Vault.");
}
