using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Entities;
using MediatR;

namespace GymBrain.Application.Orchestration.Commands;

public sealed class SaveWorkoutCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SaveWorkoutCommand, SaveWorkoutResponse>
{
    public async Task<SaveWorkoutResponse> Handle(SaveWorkoutCommand request, CancellationToken ct)
    {
        var session = new WorkoutSession(request.UserId, request.PayloadJson);
        session.MarkCompleted(); // Assuming saved = completed for now
        
        db.WorkoutSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return new SaveWorkoutResponse(session.Id);
    }
}
