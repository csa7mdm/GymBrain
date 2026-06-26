using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Orchestration.Commands;

public sealed class SaveWorkoutCommandHandler(IApplicationDbContext db, IMilestoneService milestoneService)
    : IRequestHandler<SaveWorkoutCommand, SaveWorkoutResponse>
{
    public async Task<SaveWorkoutResponse> Handle(SaveWorkoutCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user != null)
        {
            user.IncrementWorkoutsCompleted();
        }

        var session = new WorkoutSession(request.UserId, request.PayloadJson);
        session.MarkCompleted();
        
        db.WorkoutSessions.Add(session);
        await db.SaveChangesAsync(ct);

        var newUnlocks = await milestoneService.EvaluateUnlocksAsync(request.UserId, ct);

        return new SaveWorkoutResponse(session.Id, newUnlocks);
    }
}
