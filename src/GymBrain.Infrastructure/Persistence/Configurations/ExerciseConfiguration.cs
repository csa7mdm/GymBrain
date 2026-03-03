using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBrain.Infrastructure.Persistence.Configurations;

/// <summary>
/// Exercise EF Core configuration.
/// IDs are pre-assigned (ValueGeneratedNever) so they stay deterministic
/// across environments and align with the token map sent to the LLM.
/// </summary>
public sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever(); // Pre-assigned GUIDs
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.TargetMuscle).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
    }
}
