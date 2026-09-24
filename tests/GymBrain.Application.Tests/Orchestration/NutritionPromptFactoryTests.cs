using GymBrain.Application.Orchestration;

namespace GymBrain.Application.Tests.Orchestration;

public class NutritionPromptFactoryTests
{
    [Fact]
    public void MultiDayPromptPrioritizesRestrictionsAndDailyBudget()
    {
        var prompt = NutritionPromptFactory.Build("supportive", "Standard", 2000, "health", 7,
            9000, "EGP", null, null, null, null, 150, ["rice", "eggs"], "no eggs");
        Assert.Contains("exactly 7 days numbered 1 through 7", prompt);
        Assert.Contains("\"dailyBudget\":150", prompt);
        Assert.Contains("\"monthlyBudget\":null", prompt);
        Assert.Contains("no eggs", prompt);
        Assert.Contains("Respect restrictions and diet before preferred items", prompt);
        Assert.DoesNotContain("shopping_notes", prompt);
        Assert.True(prompt.Length < 2600);
    }

    [Theory]
    [InlineData("{\"days\":[{\"day_number\":1,\"meals\":[{\"name\":\"Rice\",\"ingredients\":[\"rice\"],\"steps\":[\"Cook\"]}]}]}")]
    [InlineData("{\"meals\":[{\"name\":\"Rice\",\"ingredients\":[\"rice\"],\"steps\":[\"Cook\"]}]}")]
    public void IncompleteDurationCannotReplaceSavedPlan(string response) =>
        Assert.Throws<ArgumentException>(() => NutritionPlanPayload.ValidateAndExtract(response, 2));

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
        Assert.DoesNotContain("\"reminders\":", prompt);
        Assert.True(prompt.Length < 2500);
    }
}
