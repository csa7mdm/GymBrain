using GymBrain.Domain.Enums;
using MediatR;

namespace GymBrain.Application.Orchestration.Commands;

public sealed record StartWorkoutCommand(
    Guid UserId,
    ExperienceLevel ExperienceLevel,
    string? WorkoutFocus) : IRequest<StartWorkoutResponse>;

public sealed record StartWorkoutResponse(string MegaPayloadJson);
