using GymBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Infrastructure.Persistence;

/// <summary>
/// Seeds 65 exercises covering all major muscle groups and equipment types.
/// IDs are deterministic GUIDs so the token map stays consistent.
/// Equipment tags enable LLM filtering by user's available equipment.
/// </summary>
public static class ExerciseSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>().HasData(
            // === CHEST ===
            Create("10000001-0000-0000-0000-000000000001", "Barbell Bench Press", "Chest", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000002", "Incline Barbell Bench Press", "Chest", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000003", "Dumbbell Bench Press", "Chest", "Compound", 10.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000004", "Incline Dumbbell Press", "Chest", "Compound", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000005", "Cable Fly", "Chest", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000006", "Pec Deck", "Chest", "Isolation", 15.0, "machine"),
            Create("10000001-0000-0000-0000-000000000007", "Push-Up", "Chest", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000008", "Smith Machine Flat Press", "Chest", "Compound", 20.0, "machine"),
            Create("10000001-0000-0000-0000-000000000009", "Smith Machine Incline Press", "Chest", "Compound", 15.0, "machine"),

            // === BACK ===
            Create("10000001-0000-0000-0000-000000000010", "Barbell Deadlift", "Back", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000011", "Barbell Row", "Back", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000012", "Pull-Up", "Back", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000013", "Lat Pulldown", "Back", "Compound", 25.0, "cable"),
            Create("10000001-0000-0000-0000-000000000014", "Seated Cable Row", "Back", "Compound", 20.0, "cable"),
            Create("10000001-0000-0000-0000-000000000015", "Dumbbell Row", "Back", "Compound", 10.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000016", "T-Bar Row", "Back", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000017", "Straight-Arm Pulldown", "Back", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000018", "Back Extension", "Back", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000019", "Chin-Up", "Back", "Compound", 0.0, "bodyweight"),

            // === SHOULDERS ===
            Create("10000001-0000-0000-0000-000000000020", "Overhead Press", "Shoulders", "Compound", 15.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000021", "Dumbbell Shoulder Press", "Shoulders", "Compound", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000022", "Lateral Raise", "Shoulders", "Isolation", 5.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000023", "Cable Lateral Raise", "Shoulders", "Isolation", 5.0, "cable"),
            Create("10000001-0000-0000-0000-000000000024", "Face Pull", "Shoulders", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000025", "Front Raise", "Shoulders", "Isolation", 5.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000026", "Reverse Pec Deck", "Shoulders", "Isolation", 10.0, "machine"),
            Create("10000001-0000-0000-0000-000000000027", "Arnold Press", "Shoulders", "Compound", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000028", "Cable Shrug", "Shoulders", "Isolation", 20.0, "cable"),

            // === QUADRICEPS / LEGS ===
            Create("10000001-0000-0000-0000-000000000029", "Barbell Squat", "Quadriceps", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000030", "Leg Press", "Quadriceps", "Compound", 40.0, "machine"),
            Create("10000001-0000-0000-0000-000000000031", "Leg Extension", "Quadriceps", "Isolation", 15.0, "machine"),
            Create("10000001-0000-0000-0000-000000000032", "Hack Squat", "Quadriceps", "Compound", 20.0, "machine"),
            Create("10000001-0000-0000-0000-000000000033", "Goblet Squat", "Quadriceps", "Compound", 10.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000034", "Dumbbell Lunges", "Quadriceps", "Compound", 10.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000035", "Bulgarian Split Squat", "Quadriceps", "Compound", 8.0, "dumbbell"),

            // === HAMSTRINGS / GLUTES ===
            Create("10000001-0000-0000-0000-000000000036", "Romanian Deadlift", "Hamstrings", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000037", "Leg Curl", "Hamstrings", "Isolation", 15.0, "machine"),
            Create("10000001-0000-0000-0000-000000000038", "Hip Thrust", "Glutes", "Compound", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000039", "Glute Bridge", "Glutes", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000040", "Cable Pull-Through", "Glutes", "Compound", 15.0, "cable"),

            // === BICEPS ===
            Create("10000001-0000-0000-0000-000000000041", "Barbell Curl", "Biceps", "Isolation", 10.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000042", "Dumbbell Curl", "Biceps", "Isolation", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000043", "Incline Dumbbell Curl", "Biceps", "Isolation", 6.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000044", "Hammer Curl", "Biceps", "Isolation", 8.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000045", "Cable Curl", "Biceps", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000046", "Preacher Curl", "Biceps", "Isolation", 8.0, "barbell"),

            // === TRICEPS ===
            Create("10000001-0000-0000-0000-000000000047", "Tricep Pushdown", "Triceps", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000048", "Skull Crushers", "Triceps", "Isolation", 10.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000049", "Cable Overhead Tricep Extension", "Triceps", "Isolation", 10.0, "cable"),
            Create("10000001-0000-0000-0000-000000000050", "Dumbbell Tricep Kickback", "Triceps", "Isolation", 5.0, "dumbbell"),
            Create("10000001-0000-0000-0000-000000000051", "Close-Grip Bench Press", "Triceps", "Compound", 15.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000052", "Dips", "Triceps", "Compound", 0.0, "bodyweight"),

            // === CORE ===
            Create("10000001-0000-0000-0000-000000000053", "Plank", "Core", "Bodyweight", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000054", "Cable Crunch", "Core", "Isolation", 15.0, "cable"),
            Create("10000001-0000-0000-0000-000000000055", "Hanging Leg Raise", "Core", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000056", "Ab Wheel Rollout", "Core", "Compound", 0.0, "bodyweight"),
            Create("10000001-0000-0000-0000-000000000057", "Russian Twist", "Core", "Isolation", 5.0, "dumbbell"),

            // === CALVES ===
            Create("10000001-0000-0000-0000-000000000058", "Standing Calf Raise", "Calves", "Isolation", 20.0, "machine"),
            Create("10000001-0000-0000-0000-000000000059", "Seated Calf Raise", "Calves", "Isolation", 15.0, "machine"),

            // === TRAPS ===
            Create("10000001-0000-0000-0000-000000000060", "Barbell Shrug", "Traps", "Isolation", 20.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000061", "Dumbbell Shrug", "Traps", "Isolation", 15.0, "dumbbell"),

            // === FOREARMS ===
            Create("10000001-0000-0000-0000-000000000062", "Wrist Curl", "Forearms", "Isolation", 5.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000063", "Reverse Wrist Curl", "Forearms", "Isolation", 3.0, "barbell"),

            // === FULL BODY / COMPOUND ===
            Create("10000001-0000-0000-0000-000000000064", "Clean and Press", "Full Body", "Compound", 15.0, "barbell"),
            Create("10000001-0000-0000-0000-000000000065", "Burpees", "Full Body", "Compound", 0.0, "bodyweight")
        );
    }

    private static Exercise Create(string id, string name, string targetMuscle, string category, double beginnerWeight, string equipment)
    {
        return new Exercise(Guid.Parse(id), name, targetMuscle, category, beginnerWeight, equipment);
    }
}
