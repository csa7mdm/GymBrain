using GymBrain.Application.Orchestration;
using GymBrain.Domain.Entities;

namespace GymBrain.Application.Tests.Orchestration;

/// <summary>
/// Tests for the InjuryFilter pre-filter (Phase 1A).
/// These use real exercise entities to validate keyword matching and ID exclusion.
/// </summary>
public class InjuryFilterTests
{
    // Helper: create a test exercise using the same IDs as ExerciseSeeder
    private static Exercise MakeExercise(string id, string name, string muscle, string category, string equipment)
        => new(Guid.Parse(id), name, muscle, category, 10.0, equipment);

    private static readonly List<Exercise> AllExercises = new()
    {
        // Main exercises
        MakeExercise("10000001-0000-0000-0000-000000000001", "Barbell Squat", "Quadriceps", "Compound", "barbell"),
        MakeExercise("10000001-0000-0000-0000-000000000002", "Barbell Deadlift", "Back", "Compound", "barbell"),
        MakeExercise("10000001-0000-0000-0000-000000000003", "Barbell Bench Press", "Chest", "Compound", "barbell"),
        MakeExercise("10000001-0000-0000-0000-000000000006", "Pull-Up", "Back", "Compound", "bodyweight"),
        MakeExercise("10000001-0000-0000-0000-000000000007", "Dumbbell Lunges", "Quadriceps", "Compound", "dumbbell"),
        MakeExercise("10000001-0000-0000-0000-000000000010", "Leg Press", "Quadriceps", "Compound", "machine"),
        MakeExercise("10000001-0000-0000-0000-000000000013", "Plank", "Core", "Bodyweight", "bodyweight"),
        MakeExercise("10000001-0000-0000-0000-000000000034", "Leg Extension", "Quadriceps", "Isolation", "machine"),
        MakeExercise("10000001-0000-0000-0000-000000000036", "Goblet Squat", "Quadriceps", "Compound", "dumbbell"),
        MakeExercise("10000001-0000-0000-0000-000000000037", "Bulgarian Split Squat", "Quadriceps", "Compound", "dumbbell"),
        // Warm-up exercises (should always be excluded from the LLM catalog)
        MakeExercise("10000001-0000-0000-0000-000000000066", "Treadmill Walk", "Full Body", "Warmup", "cardio"),
        MakeExercise("10000001-0000-0000-0000-000000000068", "Jumping Jacks", "Full Body", "Warmup", "bodyweight"),
        MakeExercise("10000001-0000-0000-0000-000000000070", "Bodyweight Air Squats", "Legs", "Warmup", "bodyweight"),
    };

    [Fact]
    public void Filter_WithNoInjuries_Returns_AllMainExercises_Excluding_Warmups()
    {
        var result = InjuryFilter.Filter(AllExercises, null);

        // Should have all main exercises
        Assert.Contains(result, e => e.Name == "Barbell Squat");
        Assert.Contains(result, e => e.Name == "Pull-Up");

        // Should NOT have warmups
        Assert.DoesNotContain(result, e => e.Category == "Warmup");
        Assert.DoesNotContain(result, e => e.Name == "Treadmill Walk");
        Assert.DoesNotContain(result, e => e.Name == "Jumping Jacks");
        Assert.DoesNotContain(result, e => e.Name == "Bodyweight Air Squats");
    }

    [Fact]
    public void Filter_WithEmptyInjuries_Returns_AllMainExercises_Excluding_Warmups()
    {
        var result = InjuryFilter.Filter(AllExercises, "   ");

        Assert.Contains(result, e => e.Name == "Barbell Squat");
        Assert.DoesNotContain(result, e => e.Category == "Warmup");
    }

    [Theory]
    [InlineData("bad knees")]
    [InlineData("knee pain")]
    [InlineData("ACL injury")]
    [InlineData("meniscus issue")]
    public void Filter_WithKneeInjury_Excludes_KneeLoadingExercises(string injuries)
    {
        var result = InjuryFilter.Filter(AllExercises, injuries);

        // Barbell Squat, Leg Press, Goblet Squat, Bulgarian Split Squat, Dumbbell Lunges should be excluded
        Assert.DoesNotContain(result, e => e.Name == "Barbell Squat");
        Assert.DoesNotContain(result, e => e.Name == "Leg Press");
        Assert.DoesNotContain(result, e => e.Name == "Goblet Squat");
        Assert.DoesNotContain(result, e => e.Name == "Bulgarian Split Squat");
        Assert.DoesNotContain(result, e => e.Name == "Dumbbell Lunges");

        // Safe exercises should remain
        Assert.Contains(result, e => e.Name == "Pull-Up");
        Assert.Contains(result, e => e.Name == "Plank");
        Assert.Contains(result, e => e.Name == "Barbell Bench Press");
    }

    [Theory]
    [InlineData("lower back pain")]
    [InlineData("herniated disc")]
    [InlineData("sciatica")]
    [InlineData("lumbar issue")]
    public void Filter_WithLowerBackInjury_Excludes_SpinalLoadingExercises(string injuries)
    {
        var result = InjuryFilter.Filter(AllExercises, injuries);

        // Barbell Squat, Barbell Deadlift should be excluded (spinal loading)
        Assert.DoesNotContain(result, e => e.Name == "Barbell Squat");
        Assert.DoesNotContain(result, e => e.Name == "Barbell Deadlift");

        // Upper body and safe exercises should remain
        Assert.Contains(result, e => e.Name == "Barbell Bench Press");
        Assert.Contains(result, e => e.Name == "Pull-Up");
    }

    [Fact]
    public void Filter_WithKneeAndShoulderInjury_Applies_UnionExclusion()
    {
        var exercises = new List<Exercise>(AllExercises)
        {
            MakeExercise("10000001-0000-0000-0000-000000000004", "Overhead Press", "Shoulders", "Compound", "barbell"),
        };

        var result = InjuryFilter.Filter(exercises, "bad knees and shoulder pain");

        // Knee exclusions should apply
        Assert.DoesNotContain(result, e => e.Name == "Barbell Squat");
        Assert.DoesNotContain(result, e => e.Name == "Leg Press");

        // Shoulder exclusions should also apply
        Assert.DoesNotContain(result, e => e.Name == "Overhead Press");

        // Safe exercises remain
        Assert.Contains(result, e => e.Name == "Plank");
        Assert.Contains(result, e => e.Name == "Barbell Bench Press");
    }

    [Fact]
    public void Filter_WithNoInjuries_Returns_AllMainExercises_Count()
    {
        var result = InjuryFilter.Filter(AllExercises, null);

        // 10 main exercises in our test set (not 3 warmups)
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public void GetExcludedIds_WithKneeKeyword_Returns_NonEmpty()
    {
        var ids = InjuryFilter.GetExcludedIds("I have knee pain");

        // Should return exercise IDs from the knee group
        Assert.NotEmpty(ids);
        // Barbell Squat ID should be excluded for knee pain
        Assert.Contains("10000001-0000-0000-0000-000000000001", ids, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetExcludedIds_WithNoInjuries_Returns_Empty()
    {
        var ids = InjuryFilter.GetExcludedIds(null);
        Assert.Empty(ids);
    }
}
