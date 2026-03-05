using FluentAssertions;
using GymBrain.Application.Orchestration;
using GymBrain.Domain.Entities;

namespace GymBrain.Application.Tests.Orchestration;

public class SystemPromptFactoryTests
{
    private static readonly List<Exercise> Exercises =
    [
        new(Guid.Parse("10000001-0000-0000-0000-000000000001"), "Barbell Squat", "Quadriceps", "Compound", 20.0),
        new(Guid.Parse("10000001-0000-0000-0000-000000000002"), "Barbell Deadlift", "Back", "Compound", 20.0),
    ];

    [Fact]
    public void TokenMap_Should_Use_Compressed_Format()
    {
        var map = SystemPromptFactory.BuildTokenMap(Exercises);
        map.Should().Contain("10000001-0000-0000-0000-000000000001|Barbell Squat");
        map.Should().Contain("10000001-0000-0000-0000-000000000002|Barbell Deadlift");
        // Should NOT contain description, muscle, or category (token savings)
        map.Should().NotContain("Quadriceps");
        map.Should().NotContain("Compound");
    }

    [Fact]
    public void Build_Should_Include_Persona()
    {
        var prompt = SystemPromptFactory.Build("Drill Sergeant", Exercises);
        prompt.Should().Contain("Drill Sergeant");
    }

    [Fact]
    public void Build_Should_Include_Anti_Hallucination_Rules()
    {
        var prompt = SystemPromptFactory.Build("Coach", Exercises);
        prompt.Should().Contain("MUST match an entry from the EXERCISES list");
        prompt.Should().Contain("Never invent");
    }

    [Fact]
    public void TokenMap_Should_Be_Compact()
    {
        // With 2 exercises, token map should be very short
        var map = SystemPromptFactory.BuildTokenMap(Exercises);
        var lines = map.Split('\n');
        lines.Should().HaveCount(2);
    }
}
