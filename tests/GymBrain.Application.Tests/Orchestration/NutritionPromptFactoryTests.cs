using GymBrain.Application.Orchestration;

namespace GymBrain.Application.Tests.Orchestration;

public class NutritionPromptFactoryTests
{
    [Fact]
    public void OneDayRecipePromptUsesCompactCardSchema()
    {
        var prompt = NutritionPromptFactory.Build(
            "supportive coach", "Standard", 2000, "endurance", 1,
            null, null, null, null, null, null);

        Assert.Contains("exactly three complete meals", prompt);
        Assert.Contains("ingredients", prompt);
        Assert.Contains("steps", prompt);
        Assert.DoesNotContain("shopping_notes", prompt);
        Assert.DoesNotContain("reminders", prompt);
        Assert.True(prompt.Length < 2500);
    }
}
