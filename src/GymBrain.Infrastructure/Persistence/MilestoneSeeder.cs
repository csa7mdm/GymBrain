using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Infrastructure.Persistence;

public static class MilestoneSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Chapter 1: The First Step
        modelBuilder.Entity<Milestone>().HasData(
            new Milestone(Guid.Parse("11111111-1111-1111-1111-111111111111"), "The Commitment", "Complete your first workout session.", 1, "WorkoutCount", "1"),
            new Milestone(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Building Momentum", "Complete 3 total workouts.", 1, "WorkoutCount", "3")
        );

        // Chapter 2: Finding Your Rhythm
        modelBuilder.Entity<Milestone>().HasData(
            new Milestone(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Double Down", "Complete 2 workouts in a single week.", 2, "WorkoutsInWeek", "2"),
            new Milestone(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Consistency King", "Maintain a 3-day workout streak.", 2, "Streak", "3")
        );

        // Chapter 3: The Regular
        modelBuilder.Entity<Milestone>().HasData(
            new Milestone(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Veteran", "Complete 15 total workouts.", 3, "WorkoutCount", "15")
        );
    }
}
