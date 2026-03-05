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
    public string? LlmProvider { get; private set; }
    public string? PreferredModel { get; private set; }
    public string TonePersona { get; private set; } = "Motivational Coach";
    public ExperienceLevel ExperienceLevel { get; private set; } = ExperienceLevel.Beginner;

    public void VaultApiKey(string encryptedKey, string provider, string? model = null)
    {
        EncryptedApiKey = encryptedKey ?? throw new ArgumentNullException(nameof(encryptedKey));
        LlmProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        PreferredModel = model;
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

    // === Profile persistence fields ===
    public string? Goal { get; private set; }
    public string? EquipmentJson { get; private set; }  // JSON array of available equipment
    public string? Injuries { get; private set; }
    public int DaysPerWeek { get; private set; } = 3;
    public string? DietaryPreference { get; private set; }
    public int DailyCalories { get; private set; }

    public void UpdateProfile(
        string? goal, string? equipmentJson, string? injuries,
        int daysPerWeek, string? dietaryPreference, int dailyCalories,
        ExperienceLevel experienceLevel)
    {
        Goal = goal;
        EquipmentJson = equipmentJson;
        Injuries = injuries;
        DaysPerWeek = Math.Clamp(daysPerWeek, 1, 7);
        DietaryPreference = dietaryPreference;
        DailyCalories = Math.Clamp(dailyCalories, 0, 10000);
        ExperienceLevel = experienceLevel;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
