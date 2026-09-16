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
        // Old clients without a session ID remain compatible during rollout.
        var sessionId = request.SessionId ?? Guid.NewGuid();
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var existing = await db.WorkoutSessions.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
            if (existing != null)
            {
                if (existing.UserId != request.UserId)
                    throw new InvalidOperationException("Session ID unavailable. Start a new session.");
                // First accepted completion is immutable, including after a lost response.
                return new SaveWorkoutResponse(existing.Id,
                    await milestoneService.EvaluateUnlocksAsync(request.UserId, ct));
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                ?? throw new InvalidOperationException("User not found.");
            user.IncrementWorkoutsCompleted();
            var session = new WorkoutSession(request.UserId, request.PayloadJson, sessionId);
            session.MarkCompleted();
            db.WorkoutSessions.Add(session);
            try
            {
                // EF commits the insert and counter together. The counter concurrency
                // token prevents lost increments from different simultaneous sessions.
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException) when (attempt < 2)
            {
                db.ChangeTracker.Clear();
                continue;
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                // A competing retry may have inserted the same primary key.
                if (await db.WorkoutSessions.AsNoTracking()
                    .AnyAsync(s => s.Id == sessionId && s.UserId == request.UserId, ct))
                    return new SaveWorkoutResponse(sessionId,
                        await milestoneService.EvaluateUnlocksAsync(request.UserId, ct));
                throw;
            }
            return new SaveWorkoutResponse(sessionId,
                await milestoneService.EvaluateUnlocksAsync(request.UserId, ct));
        }
        throw new InvalidOperationException("Unable to save right now. Retry this session.");
    }
}
