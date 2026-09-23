using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Tests.Auth;

internal sealed class AuthTestDb() : DbContext(new DbContextOptionsBuilder<AuthTestDb>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Exercise> Exercises => throw new NotSupportedException();
    public DbSet<WorkoutSession> WorkoutSessions => throw new NotSupportedException();
    public DbSet<NutritionPlan> NutritionPlans => throw new NotSupportedException();
    public DbSet<AnalyticsEvent> AnalyticsEvents => throw new NotSupportedException();
    public DbSet<Milestone> Milestones => throw new NotSupportedException();
    public DbSet<UserMilestone> UserMilestones => throw new NotSupportedException();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Ignore(u => u.CurrentChapter).Ignore(u => u.DomainEvents);
    }
}

internal sealed class TestPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => "hashed:" + password;
    public bool Verify(string password, string hash) => Hash(password) == hash;
}

internal sealed class TestJwtService : IJwtTokenService
{
    public int Calls { get; private set; }
    public string GenerateToken(User user) { Calls++; return "test-token"; }
}
