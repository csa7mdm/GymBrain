using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBrain.Infrastructure.Persistence.Configurations;

public sealed class NutritionPlanConfiguration : IEntityTypeConfiguration<NutritionPlan>
{
    public void Configure(EntityTypeBuilder<NutritionPlan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PayloadJson).IsRequired();
        builder.HasIndex(p => new { p.UserId, p.CreatedAtUtc });
        builder.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
