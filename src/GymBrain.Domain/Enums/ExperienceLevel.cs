namespace GymBrain.Domain.Enums;

/// <summary>
/// Determines Safety Gate weight thresholds.
/// Beginner: max 40kg cap enforced by SafetyGate.
/// </summary>
public enum ExperienceLevel
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
    Athlete = 3
}
