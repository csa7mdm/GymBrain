using GymBrain.Domain.Entities;

namespace GymBrain.Application.Common.Interfaces;

public interface IMilestoneService
{
    Task<List<Milestone>> EvaluateUnlocksAsync(Guid userId, CancellationToken ct = default);
}
