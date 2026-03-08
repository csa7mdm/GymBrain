using MediatR;
using GymBrain.Domain.Common;
using GymBrain.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Telemetry.Queries;

public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public GetMetricsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalWorkouts = await _context.WorkoutSessions.CountAsync(cancellationToken);
        
        var eventCounts = await _context.AnalyticsEvents
            .GroupBy(e => e.EventName)
            .Select(g => new { EventName = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var metrics = new
        {
            TotalUsers = totalUsers,
            TotalWorkouts = totalWorkouts,
            Events = eventCounts,
            GeneratedAt = DateTime.UtcNow
        };

        return Result<object>.Success(metrics);
    }
}
