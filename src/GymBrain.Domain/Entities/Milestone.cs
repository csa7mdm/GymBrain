using GymBrain.Domain.Common;

namespace GymBrain.Domain.Entities;

public class Milestone : BaseEntity
{
    private Milestone() { } // EF Core

    public Milestone(Guid id, string name, string description, int chapterNumber, string conditionType, string conditionValue)
    {
        Id = id;
        Name = name;
        Description = description;
        ChapterNumber = chapterNumber;
        ConditionType = conditionType;
        ConditionValue = conditionValue;
    }

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int ChapterNumber { get; private set; }
    public string ConditionType { get; private set; } = null!; // e.g., "WorkoutCount", "UniqueExercise", "WeightIncrease"
    public string ConditionValue { get; private set; } = null!; // e.g., "3", "5", "True"
}
