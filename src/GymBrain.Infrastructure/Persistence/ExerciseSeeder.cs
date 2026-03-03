using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Infrastructure.Persistence;

/// <summary>
/// Seeds 15 core exercises at startup.
/// This fixes the previous issue where the Orchestrator crashed with
/// "No exercises available in the domain" because the table was empty.
/// IDs are deterministic GUIDs so the token map stays consistent.
/// </summary>
public static class ExerciseSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>().HasData(
            Create("10000001-0000-0000-0000-000000000001", "Barbell Squat", "Quadriceps", "Compound", 20.0),
            Create("10000001-0000-0000-0000-000000000002", "Barbell Deadlift", "Back", "Compound", 20.0),
            Create("10000001-0000-0000-0000-000000000003", "Barbell Bench Press", "Chest", "Compound", 20.0),
            Create("10000001-0000-0000-0000-000000000004", "Overhead Press", "Shoulders", "Compound", 15.0),
            Create("10000001-0000-0000-0000-000000000005", "Barbell Row", "Back", "Compound", 20.0),
            Create("10000001-0000-0000-0000-000000000006", "Pull-Up", "Back", "Bodyweight", 0.0),
            Create("10000001-0000-0000-0000-000000000007", "Dumbbell Lunges", "Quadriceps", "Compound", 10.0),
            Create("10000001-0000-0000-0000-000000000008", "Dumbbell Curl", "Biceps", "Isolation", 8.0),
            Create("10000001-0000-0000-0000-000000000009", "Tricep Pushdown", "Triceps", "Isolation", 10.0),
            Create("10000001-0000-0000-0000-000000000010", "Leg Press", "Quadriceps", "Compound", 40.0),
            Create("10000001-0000-0000-0000-000000000011", "Lat Pulldown", "Back", "Compound", 25.0),
            Create("10000001-0000-0000-0000-000000000012", "Cable Fly", "Chest", "Isolation", 10.0),
            Create("10000001-0000-0000-0000-000000000013", "Plank", "Core", "Bodyweight", 0.0),
            Create("10000001-0000-0000-0000-000000000014", "Romanian Deadlift", "Hamstrings", "Compound", 20.0),
            Create("10000001-0000-0000-0000-000000000015", "Lateral Raise", "Shoulders", "Isolation", 5.0)
        );
    }

    private static Exercise Create(string id, string name, string targetMuscle, string category, double beginnerWeight)
    {
        return new Exercise(Guid.Parse(id), name, targetMuscle, category, beginnerWeight);
    }
}
