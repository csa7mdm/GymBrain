using GymBrain.Domain.Common;

namespace GymBrain.Domain.Entities;

/// <summary>
/// Represents a catalog exercise in the domain. These are seeded at startup
/// and used by the Orchestrator to build token-compressed prompts (ID|Name format).
/// </summary>
public class Exercise : BaseEntity
{
    private Exercise() { } // EF Core

    public Exercise(Guid id, string name, string targetMuscle, string category, double defaultBeginnerWeightKg, string equipment = "bodyweight")
    {
        Id = id;
        Name = name;
        TargetMuscle = targetMuscle;
        Category = category;
        DefaultBeginnerWeightKg = defaultBeginnerWeightKg;
        Equipment = equipment;
    }

    public string Name { get; private set; } = null!;
    public string TargetMuscle { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public double DefaultBeginnerWeightKg { get; private set; }
    public string Equipment { get; private set; } = "bodyweight";
}
