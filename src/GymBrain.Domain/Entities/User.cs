using GymBrain.Domain.Common;
using GymBrain.Domain.Enums;

namespace GymBrain.Domain.Entities;

public class User : BaseEntity
{
    private User() { } // EF Core

    public NarrativeChapter CurrentChapter => NarrativeProgress.GetChapter(WorkoutsCompleted);

    public User(string email, string passwordHash, ExperienceLevel experienceLevel)
    {
        Email = email;
        PasswordHash = passwordHash;
        ExperienceLevel = experienceLevel;
    }

    public string Email { get; private set; } = null!;
    public string? FirebaseUid { get; private set; }
    public void LinkFirebase(string uid)
    {
        if (string.IsNullOrWhiteSpace(uid) || uid.Length > 128) throw new ArgumentException("Invalid identity.");
        if (FirebaseUid != null && FirebaseUid != uid) throw new InvalidOperationException("Account already linked.");
        FirebaseUid = uid;
        // Retain the legacy hash for a reversible rollout; linked accounts use Firebase sign-in.
        UpdatedAtUtc = DateTime.UtcNow;
    }
    public string PasswordHash { get; private set; } = null!;
    public string? EncryptedApiKey { get; private set; }
    public string? LlmProvider { get; private set; }
    public string? PreferredModel { get; private set; }

    public void UpdatePassword(string hash)
    {
        PasswordHash = hash;
        UpdatedAtUtc = DateTime.UtcNow;
    }
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

    public string? DisplayName { get; private set; }
    public int? Age { get; private set; }
    public double? HeightCm { get; private set; }
    public double? WeightKg { get; private set; }
    public string? FocusAreasJson { get; private set; }

    public void UpdatePersonalProfile(string name, int age, double height, double weight, string[] focusAreas)
    {
        string[] allowedFocus = ["Chest", "Back", "Shoulders", "Arms", "Legs", "Core", "Glutes", "Full Body"];
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100 || name.Any(char.IsControl))
            throw new ArgumentException("Enter a name between 1 and 100 characters.");
        if (age < 14 || age > 100 || !double.IsFinite(height) || height < 100 || height > 250 ||
            !double.IsFinite(weight) || weight < 25 || weight > 350)
            throw new ArgumentException("Check your age, height and weight.");
        if (focusAreas == null || focusAreas.Length > allowedFocus.Length || focusAreas.Any(f => !allowedFocus.Contains(f)))
            throw new ArgumentException("Select valid focus areas.");
        DisplayName = name.Trim(); Age = age; HeightCm = height; WeightKg = weight;
        FocusAreasJson = System.Text.Json.JsonSerializer.Serialize(focusAreas.Distinct());
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // === Profile & Usage persistence fields ===
    public int WorkoutsCompleted { get; private set; } = 0;
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

    public void IncrementWorkoutsCompleted()
    {
        WorkoutsCompleted++;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
