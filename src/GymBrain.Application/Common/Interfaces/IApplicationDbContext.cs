using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// EF Core abstraction — keeps Infrastructure details out of Application layer.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Exercise> Exercises { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
