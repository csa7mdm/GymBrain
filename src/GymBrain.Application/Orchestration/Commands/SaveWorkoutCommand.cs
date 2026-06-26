using GymBrain.Domain.Entities;
using MediatR;

namespace GymBrain.Application.Orchestration.Commands;

public sealed record SaveWorkoutCommand(
    Guid UserId,
    string PayloadJson) : IRequest<SaveWorkoutResponse>;

public sealed record SaveWorkoutResponse(
    Guid WorkoutSessionId, 
    List<Milestone> UnlockedMilestones);
