using GymBrain.Domain.Common;
using GymBrain.Domain.Enums;

namespace GymBrain.Domain.Entities;

/// <summary>
/// User aggregate root. Holds identity, hashed credentials,
/// encrypted BYO-API key (AES-256), and personalization preferences.
/// </summary>
public class User : BaseEntity
{
    private User() { } // EF Core

    public User(string email, string passwordHash, ExperienceLevel experienceLevel)
    {
        Email = email;
        PasswordHash = passwordHash;
        ExperienceLevel = experienceLevel;
    }

    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? EncryptedApiKey { get; private set; }
    public string TonePersona { get; private set; } = "Motivational Coach";
    public ExperienceLevel ExperienceLevel { get; private set; } = ExperienceLevel.Beginner;

    public void VaultApiKey(string encryptedKey)
    {
        EncryptedApiKey = encryptedKey ?? throw new ArgumentNullException(nameof(encryptedKey));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetTonePersona(string persona)
    {
        if (string.IsNullOrWhiteSpace(persona))
            throw new ArgumentException("Persona cannot be empty.", nameof(persona));

        TonePersona = persona;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetExperienceLevel(ExperienceLevel level)
    {
        ExperienceLevel = level;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
