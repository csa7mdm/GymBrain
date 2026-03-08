using MediatR;
using GymBrain.Domain.Common;
using GymBrain.Domain.Entities;
using GymBrain.Application.Common.Interfaces;

namespace GymBrain.Application.Telemetry.Commands;

public class TrackEventCommandHandler : IRequestHandler<TrackEventCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public TrackEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(TrackEventCommand request, CancellationToken cancellationToken)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            UserId = request.UserId,
            EventName = request.EventName,
            MetadataJson = request.Metadata.HasValue && request.Metadata.Value.ValueKind != System.Text.Json.JsonValueKind.Undefined && request.Metadata.Value.ValueKind != System.Text.Json.JsonValueKind.Null
                ? request.Metadata.Value.GetRawText()
                : null
        };

        _context.AnalyticsEvents.Add(analyticsEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(analyticsEvent.Id.ToString());
    }
}
