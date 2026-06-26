using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Infrastructure.Services;

public class MilestoneService(IApplicationDbContext db) : IMilestoneService
{
    public async Task<List<Milestone>> EvaluateUnlocksAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return new List<Milestone>();

        // Get already unlocked milestone IDs
        var unlockedIds = await db.UserMilestones
            .Where(um => um.UserId == userId)
            .Select(um => um.MilestoneId)
            .ToListAsync(ct);

        // Get all milestones not yet unlocked
        var pendingMilestones = await db.Milestones
            .Where(m => !unlockedIds.Contains(m.Id))
            .ToListAsync(ct);

        var newUnlocks = new List<Milestone>();

        foreach (var milestone in pendingMilestones)
        {
            bool isUnlocked = milestone.ConditionType switch
            {
                "WorkoutCount" => user.WorkoutsCompleted >= int.Parse(milestone.ConditionValue),
                // Add more complex logic here for Streak, unique exercises, etc.
                "Streak" => false, // placeholder
                _ => false
            };

            if (isUnlocked)
            {
                db.UserMilestones.Add(new UserMilestone(userId, milestone.Id));
                newUnlocks.Add(milestone);
            }
        }

        if (newUnlocks.Any())
        {
            await db.SaveChangesAsync(ct);
        }

        return newUnlocks;
    }
}
