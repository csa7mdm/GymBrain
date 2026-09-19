using MediatR;

namespace GymBrain.Application.Profile.Commands;

public record SaveProfileCommand(
    Guid UserId,
    string? Goal,
    string? EquipmentJson,
    string? Injuries,
    int DaysPerWeek,
    string? DietaryPreference,
    int DailyCalories,
    string ExperienceLevel,
    PersonalProfile? PersonalProfile = null) : IRequest<SaveProfileResponse>;

public record PersonalProfile(string Name, int Age, double Height, double Weight, string[] FocusAreas);

public record SaveProfileResponse(string Message);
