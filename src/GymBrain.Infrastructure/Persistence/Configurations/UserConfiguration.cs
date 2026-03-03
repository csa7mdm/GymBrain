using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBrain.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.EncryptedApiKey).HasMaxLength(2048);
        builder.Property(u => u.TonePersona).HasMaxLength(100).HasDefaultValue("Motivational Coach");

        // Exclude EncryptedApiKey from default queries for security
        builder.Property(u => u.EncryptedApiKey).HasColumnName("encrypted_api_key");
    }
}
