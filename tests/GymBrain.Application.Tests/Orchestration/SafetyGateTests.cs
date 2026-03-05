using FluentAssertions;
using GymBrain.Application.Orchestration;
using GymBrain.Domain.Entities;
using GymBrain.Domain.Enums;

namespace GymBrain.Application.Tests.Orchestration;

public class SafetyGateTests
{
    private static readonly List<Exercise> ValidExercises =
    [
        new(Guid.Parse("10000001-0000-0000-0000-000000000001"), "Barbell Squat", "Quadriceps", "Compound", 20.0),
        new(Guid.Parse("10000001-0000-0000-0000-000000000002"), "Barbell Deadlift", "Back", "Compound", 20.0),
    ];

    [Fact]
    public void Should_Clamp_Weight_For_Beginner()
    {
        var json = """{"exercise_id": "10000001-0000-0000-0000-000000000001", "weight_kg": 80.0}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Beginner);
        result.Should().Contain("40");
        result.Should().NotContain("80");
    }

    [Fact]
    public void Should_Replace_Hallucinated_Exercise_Id()
    {
        var json = """{"exercise_id": "00000000-0000-0000-0000-fakefakefake", "weight_kg": 20.0}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Beginner);
        result.Should().Contain("10000001-0000-0000-0000-000000000001"); // fallback to first valid
    }

    [Fact]
    public void Should_Keep_Valid_Exercise_Id()
    {
        var json = """{"exercise_id": "10000001-0000-0000-0000-000000000002", "weight_kg": 20.0}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Beginner);
        result.Should().Contain("10000001-0000-0000-0000-000000000002");
    }

    [Fact]
    public void Should_Allow_Higher_Weight_For_Advanced()
    {
        var json = """{"exercise_id": "10000001-0000-0000-0000-000000000001", "weight_kg": 150.0}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Advanced);
        result.Should().Contain("150");
    }

    [Fact]
    public void Should_Clamp_Excessive_Reps()
    {
        var json = """{"exercise_id": "10000001-0000-0000-0000-000000000001", "reps": 100}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Beginner);
        result.Should().Contain("\"reps\": 30");
    }

    [Fact]
    public void Should_Clamp_Low_Rest_Seconds()
    {
        var json = """{"exercise_id": "10000001-0000-0000-0000-000000000001", "rest_seconds": 5}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Beginner);
        result.Should().Contain("30");
    }

    [Fact]
    public void Should_Clamp_High_Rest_Seconds()
    {
        var json = """{"exercise_id": "10000001-0000-0000-0000-000000000001", "rest_seconds": 600}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Beginner);
        result.Should().Contain("300");
        result.Should().NotContain("600");
    }

    [Fact]
    public void Should_Clamp_Excessive_Sets()
    {
        var json = """{"exercise_id": "10000001-0000-0000-0000-000000000001", "sets": 20}""";
        var result = SafetyGate.Validate(json, ValidExercises, ExperienceLevel.Beginner);
        result.Should().Contain("10");
        result.Should().NotContain("20");
    }
}
