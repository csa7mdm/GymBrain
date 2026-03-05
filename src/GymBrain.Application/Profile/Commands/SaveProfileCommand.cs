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
    string ExperienceLevel) : IRequest<SaveProfileResponse>;

public record SaveProfileResponse(string Message);
