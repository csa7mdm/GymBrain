using GymBrain.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Profile.Queries;

public record GetProfileQuery(Guid UserId) : IRequest<GetProfileResponse>;

public record GetProfileResponse(
    string? Goal,
    string? EquipmentJson,
    string? Injuries,
    int DaysPerWeek,
    string? DietaryPreference,
    int DailyCalories,
    string ExperienceLevel,
    string? TonePersona);

public sealed class GetProfileQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProfileQuery, GetProfileResponse>
{
    public async Task<GetProfileResponse> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        return new GetProfileResponse(
            user.Goal,
            user.EquipmentJson,
            user.Injuries,
            user.DaysPerWeek,
            user.DietaryPreference,
            user.DailyCalories,
            user.ExperienceLevel.ToString(),
            user.TonePersona);
    }
}
