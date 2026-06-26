using GymBrain.Domain.Common;

namespace GymBrain.Domain.Entities;

public class UserMilestone : BaseEntity
{
    private UserMilestone() { } // EF Core

    public UserMilestone(Guid userId, Guid milestoneId)
    {
        UserId = userId;
        MilestoneId = milestoneId;
        UnlockedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid MilestoneId { get; private set; }
    public DateTime UnlockedAtUtc { get; private set; }
}
