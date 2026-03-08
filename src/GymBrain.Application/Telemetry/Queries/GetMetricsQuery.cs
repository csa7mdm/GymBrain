using MediatR;
using GymBrain.Domain.Common;

namespace GymBrain.Application.Telemetry.Queries;

public record GetMetricsQuery : IRequest<Result<object>>;
