using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Profile.Commands;

public record IncrementWorkoutsCommand(Guid UserId) : IRequest<int>;

public sealed class IncrementWorkoutsCommandHandler(IApplicationDbContext db)
    : IRequestHandler<IncrementWorkoutsCommand, int>
{
    public async Task<int> Handle(IncrementWorkoutsCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        user.IncrementWorkoutsCompleted();
        await db.SaveChangesAsync(ct);

        return user.WorkoutsCompleted;
    }
}
