using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Profile.Commands;

public sealed class SaveProfileCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SaveProfileCommand, SaveProfileResponse>
{
    public async Task<SaveProfileResponse> Handle(SaveProfileCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        var level = Enum.TryParse<ExperienceLevel>(request.ExperienceLevel, ignoreCase: true, out var parsed)
            ? parsed : ExperienceLevel.Beginner;

        if (request.PersonalProfile is { } personal)
            user.UpdatePersonalProfile(personal.Name, personal.Age, personal.Height, personal.Weight, personal.FocusAreas);

        user.UpdateProfile(
            request.Goal,
            request.EquipmentJson,
            request.Injuries,
            request.DaysPerWeek,
            request.DietaryPreference,
            request.DailyCalories,
            level);

        await db.SaveChangesAsync(ct);

        return new SaveProfileResponse("Profile saved successfully.");
    }
}
