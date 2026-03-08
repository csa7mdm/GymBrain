using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Infrastructure.Persistence;

/// <summary>
/// Seeds 70 exercises covering all major muscle groups and equipment types.
/// IDs are deterministic GUIDs and the first 15 are preserved to maintain
/// database migration and testing consistency.
/// Exercises with Category="Warmup" (IDs ...000066-000070) are excluded from
/// main workout generation and substitute mappings.
/// </summary>
public static class ExerciseSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>().HasData(
            // === ORIGINAL 15 (Preserved IDs to avoid migration/unique index conflicts) ===
            Create("10000001-0000-0000-0000-000000000001", "Barbell Squat", "Quadriceps", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000002", "Barbell Deadlift", "Back", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000003", "Barbell Bench Press", "Chest", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000004", "Overhead Press", "Shoulders", "Compound", 15.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000005", "Barbell Row", "Back", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000006", "Pull-Up", "Back", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000007", "Dumbbell Lunges", "Quadriceps", "Compound", 10.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000008", "Dumbbell Curl", "Biceps", "Isolation", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000009", "Tricep Pushdown", "Triceps", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000010", "Leg Press", "Quadriceps", "Compound", 40.0, "machine"),
            Create("10000001-0000-0000-0000-000000000011", "Lat Pulldown", "Back", "Compound", 25.0, "cable"),
            Create("10000001-0000-0000-0000-000000000012", "Cable Fly", "Chest", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000013", "Plank", "Core", "Bodyweight", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000014", "Romanian Deadlift", "Hamstrings", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000015", "Lateral Raise", "Shoulders", "Isolation", 5.0, "dumbbell"),

            // === EXPANDED CATALOG (50 Additional Exercises) ===
            // Chest
            Create("10000001-0000-0000-0000-000000000016", "Incline Barbell Bench Press", "Chest", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000017", "Dumbbell Bench Press", "Chest", "Compound", 10.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000018", "Incline Dumbbell Press", "Chest", "Compound", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000019", "Pec Deck", "Chest", "Isolation", 15.0, "machine"),
            Create("10000001-0000-0000-0000-000000000020", "Smith Machine Flat Press", "Chest", "Compound", 20.0, "machine"),
            Create("10000001-0000-0000-0000-000000000021", "Smith Machine Incline Press", "Chest", "Compound", 15.0, "machine"),

            // Back
            Create("10000001-0000-0000-0000-000000000022", "Seated Cable Row", "Back", "Compound", 20.0, "cable"),
            Create("10000001-0000-0000-0000-000000000023", "T-Bar Row", "Back", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000024", "Straight-Arm Pulldown", "Back", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000025", "Back Extension", "Back", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000026", "Chin-Up", "Back", "Compound", 0.0, "bodyweight"),

            // Shoulders
            Create("10000001-0000-0000-0000-000000000027", "Dumbbell Shoulder Press", "Shoulders", "Compound", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000028", "Cable Lateral Raise", "Shoulders", "Isolation", 5.0, "cable"),
            Create("10000001-0000-0000-0000-000000000029", "Face Pull", "Shoulders", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000030", "Front Raise", "Shoulders", "Isolation", 5.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000031", "Reverse Pec Deck", "Shoulders", "Isolation", 10.0, "machine"),
            Create("10000001-0000-0000-0000-000000000032", "Arnold Press", "Shoulders", "Compound", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000033", "Cable Shrug", "Shoulders", "Isolation", 20.0, "cable"),

            // Legs
            Create("10000001-0000-0000-0000-000000000034", "Leg Extension", "Quadriceps", "Isolation", 15.0, "machine"),
            Create("10000001-0000-0000-0000-000000000035", "Hack Squat", "Quadriceps", "Compound", 20.0, "machine"),
            Create("10000001-0000-0000-0000-000000000036", "Goblet Squat", "Quadriceps", "Compound", 10.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000037", "Bulgarian Split Squat", "Quadriceps", "Compound", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000038", "Leg Curl", "Hamstrings", "Isolation", 15.0, "machine"),
            Create("10000001-0000-0000-0000-000000000039", "Hip Thrust", "Glutes", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000040", "Glute Bridge", "Glutes", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000041", "Cable Pull-Through", "Glutes", "Compound", 15.0, "cable"),

            // Biceps
            Create("10000001-0000-0000-0000-000000000042", "Barbell Curl", "Biceps", "Isolation", 10.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000043", "Incline Dumbbell Curl", "Biceps", "Isolation", 6.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000044", "Hammer Curl", "Biceps", "Isolation", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000045", "Cable Curl", "Biceps", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000046", "Preacher Curl", "Biceps", "Isolation", 8.0, "barbell"),

            // Triceps
            Create("10000001-0000-0000-0000-000000000047", "Skull Crushers", "Triceps", "Isolation", 10.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000048", "Cable Overhead Tricep Extension", "Triceps", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000049", "Dumbbell Tricep Kickback", "Triceps", "Isolation", 5.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000050", "Close-Grip Bench Press", "Triceps", "Compound", 15.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000051", "Dips", "Triceps", "Compound", 0.0, "bodyweight"),

            // Core
            Create("10000001-0000-0000-0000-000000000052", "Hanging Leg Raise", "Core", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000053", "Ab Wheel Rollout", "Core", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000054", "Russian Twist", "Core", "Isolation", 5.0, "dumbbell"),

            // Other
            Create("10000001-0000-0000-0000-000000000055", "Standing Calf Raise", "Calves", "Isolation", 20.0, "machine"),
            Create("10000001-0000-0000-0000-000000000056", "Seated Calf Raise", "Calves", "Isolation", 15.0, "machine"),
            Create("10000001-0000-0000-0000-000000000057", "Barbell Shrug", "Traps", "Isolation", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000058", "Dumbbell Shrug", "Traps", "Isolation", 15.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000059", "Wrist Curl", "Forearms", "Isolation", 5.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000060", "Reverse Wrist Curl", "Forearms", "Isolation", 3.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000061", "Clean and Press", "Full Body", "Compound", 15.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000062", "Burpees", "Full Body", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000063", "Diamond Push-Up", "Triceps", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000064", "Box Jump", "Quadriceps", "Plyometric", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000065", "Medicine Ball Slam", "Full Body", "Compound", 5.0, "bodyweight"),

            // === WARM-UP EXERCISES (Category="Warmup" — excluded from main workout LLM catalog) ===
            Create("10000001-0000-0000-0000-000000000066", "Treadmill Walk", "Full Body", "Warmup", 0.0, "cardio"),
            Create("10000001-0000-0000-0000-000000000067", "Stationary Bike", "Full Body", "Warmup", 0.0, "cardio"),
            Create("10000001-0000-0000-0000-000000000068", "Jumping Jacks", "Full Body", "Warmup", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000069", "Arm Circles", "Shoulders", "Warmup", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000070", "Bodyweight Air Squats", "Legs", "Warmup", 0.0, "bodyweight")
        );
    }

    private static Exercise Create(string id, string name, string targetMuscle, string category, double beginnerWeight, string equipment)
    {
        return new Exercise(Guid.Parse(id), name, targetMuscle, category, beginnerWeight, equipment);
    }
}
