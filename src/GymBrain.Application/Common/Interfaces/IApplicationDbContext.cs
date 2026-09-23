using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// EF Core abstraction — keeps Infrastructure details out of Application layer.
/// </summary>
public interface IApplicationDbContext
{
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }
    DbSet<User> Users { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<WorkoutSession> WorkoutSessions { get; }
    DbSet<NutritionPlan> NutritionPlans { get; }
    DbSet<AnalyticsEvent> AnalyticsEvents { get; }
    DbSet<Milestone> Milestones { get; }
    DbSet<UserMilestone> UserMilestones { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
