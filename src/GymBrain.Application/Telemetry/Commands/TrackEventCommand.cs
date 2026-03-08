using MediatR;
using GymBrain.Domain.Common;
using System.Text.Json;

namespace GymBrain.Application.Telemetry.Commands;

public record TrackEventCommand(Guid UserId, string EventName, JsonElement? Metadata) : IRequest<Result<string>>;
