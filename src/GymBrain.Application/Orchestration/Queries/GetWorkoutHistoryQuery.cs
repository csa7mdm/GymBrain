using GymBrain.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Orchestration.Queries;

public record GetWorkoutHistoryQuery(Guid UserId, int Offset = 0) : IRequest<WorkoutHistoryResponse>;
public record WorkoutHistoryItem(Guid Id, DateTime CompletedAtUtc, string PayloadJson);
public record WorkoutHistoryResponse(List<WorkoutHistoryItem> Items, int Total, bool HasMore);

public sealed class GetWorkoutHistoryQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetWorkoutHistoryQuery, WorkoutHistoryResponse>
{
    public async Task<WorkoutHistoryResponse> Handle(GetWorkoutHistoryQuery request, CancellationToken ct)
    {
        if (request.Offset < 0 || request.Offset > 100000)
            throw new ArgumentException("Invalid history offset.");
        var query = db.WorkoutSessions.AsNoTracking()
            .Where(s => s.UserId == request.UserId && s.IsCompleted);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(s => s.CreatedAtUtc).ThenByDescending(s => s.Id)
            .Skip(request.Offset).Take(50)
            .Select(s => new WorkoutHistoryItem(s.Id, s.CreatedAtUtc, s.PayloadJson)).ToListAsync(ct);
        return new(items, total, request.Offset + items.Count < total);
    }
}
