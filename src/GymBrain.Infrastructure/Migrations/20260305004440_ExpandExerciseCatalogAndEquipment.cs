using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandExerciseCatalogAndEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Equipment",
                table: "Exercises",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000001"),
                columns: new[] { "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "barbell", "Barbell Bench Press", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000002"),
                columns: new[] { "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "barbell", "Incline Barbell Bench Press", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000003"),
                columns: new[] { "DefaultBeginnerWeightKg", "Equipment", "Name" },
                values: new object[] { 10.0, "dumbbell", "Dumbbell Bench Press" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000004"),
                columns: new[] { "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { 8.0, "dumbbell", "Incline Dumbbell Press", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000005"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "Isolation", 10.0, "cable", "Cable Fly", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000006"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "Isolation", 15.0, "machine", "Pec Deck", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000007"),
                columns: new[] { "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { 0.0, "bodyweight", "Push-Up", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000008"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "Compound", 20.0, "machine", "Smith Machine Flat Press", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000009"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "Compound", 15.0, "machine", "Smith Machine Incline Press", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000010"),
                columns: new[] { "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { 20.0, "barbell", "Barbell Deadlift", "Back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000011"),
                columns: new[] { "DefaultBeginnerWeightKg", "Equipment", "Name" },
                values: new object[] { 20.0, "barbell", "Barbell Row" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000012"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "Compound", 0.0, "bodyweight", "Pull-Up", "Back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000013"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "Compound", 25.0, "cable", "Lat Pulldown", "Back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000014"),
                columns: new[] { "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "cable", "Seated Cable Row", "Back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000015"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle" },
                values: new object[] { "Compound", 10.0, "dumbbell", "Dumbbell Row", "Back" });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "Category", "CreatedAtUtc", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000016"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "T-Bar Row", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000017"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Straight-Arm Pulldown", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000018"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Back Extension", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000019"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Chin-Up", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000020"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "barbell", "Overhead Press", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000021"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Dumbbell Shoulder Press", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000022"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "dumbbell", "Lateral Raise", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000023"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "cable", "Cable Lateral Raise", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000024"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Face Pull", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000025"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "dumbbell", "Front Raise", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000026"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "machine", "Reverse Pec Deck", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000027"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Arnold Press", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000028"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "cable", "Cable Shrug", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000029"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "Barbell Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000030"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 40.0, "machine", "Leg Press", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000031"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Leg Extension", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000032"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "machine", "Hack Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000033"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "dumbbell", "Goblet Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000034"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "dumbbell", "Dumbbell Lunges", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000035"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Bulgarian Split Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000036"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "Romanian Deadlift", "Hamstrings", null },
                    { new Guid("10000001-0000-0000-0000-000000000037"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Leg Curl", "Hamstrings", null },
                    { new Guid("10000001-0000-0000-0000-000000000038"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "Hip Thrust", "Glutes", null },
                    { new Guid("10000001-0000-0000-0000-000000000039"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Glute Bridge", "Glutes", null },
                    { new Guid("10000001-0000-0000-0000-000000000040"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "cable", "Cable Pull-Through", "Glutes", null },
                    { new Guid("10000001-0000-0000-0000-000000000041"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "barbell", "Barbell Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000042"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Dumbbell Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000043"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6.0, "dumbbell", "Incline Dumbbell Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000044"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Hammer Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000045"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Cable Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000046"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "barbell", "Preacher Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000047"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Tricep Pushdown", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000048"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "barbell", "Skull Crushers", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000049"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Cable Overhead Tricep Extension", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000050"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "dumbbell", "Dumbbell Tricep Kickback", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000051"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "barbell", "Close-Grip Bench Press", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000052"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Dips", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000053"), "Bodyweight", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Plank", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000054"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "cable", "Cable Crunch", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000055"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Hanging Leg Raise", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000056"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Ab Wheel Rollout", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000057"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "dumbbell", "Russian Twist", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000058"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "machine", "Standing Calf Raise", "Calves", null },
                    { new Guid("10000001-0000-0000-0000-000000000059"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Seated Calf Raise", "Calves", null },
                    { new Guid("10000001-0000-0000-0000-000000000060"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "Barbell Shrug", "Traps", null },
                    { new Guid("10000001-0000-0000-0000-000000000061"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "dumbbell", "Dumbbell Shrug", "Traps", null },
                    { new Guid("10000001-0000-0000-0000-000000000062"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "barbell", "Wrist Curl", "Forearms", null },
                    { new Guid("10000001-0000-0000-0000-000000000063"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3.0, "barbell", "Reverse Wrist Curl", "Forearms", null },
                    { new Guid("10000001-0000-0000-0000-000000000064"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "barbell", "Clean and Press", "Full Body", null },
                    { new Guid("10000001-0000-0000-0000-000000000065"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Burpees", "Full Body", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000035"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000036"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000037"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000038"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000039"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000041"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000042"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000043"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000044"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000045"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000046"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000047"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000048"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000049"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000050"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000051"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000052"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000053"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000054"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000055"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000056"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000057"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000058"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000059"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000060"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000061"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000062"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000063"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000064"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000065"));

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Exercises");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000001"),
                columns: new[] { "Name", "TargetMuscle" },
                values: new object[] { "Barbell Squat", "Quadriceps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000002"),
                columns: new[] { "Name", "TargetMuscle" },
                values: new object[] { "Barbell Deadlift", "Back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000003"),
                columns: new[] { "DefaultBeginnerWeightKg", "Name" },
                values: new object[] { 20.0, "Barbell Bench Press" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000004"),
                columns: new[] { "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { 15.0, "Overhead Press", "Shoulders" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000005"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { "Compound", 20.0, "Barbell Row", "Back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000006"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { "Bodyweight", 0.0, "Pull-Up", "Back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000007"),
                columns: new[] { "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { 10.0, "Dumbbell Lunges", "Quadriceps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000008"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { "Isolation", 8.0, "Dumbbell Curl", "Biceps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000009"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { "Isolation", 10.0, "Tricep Pushdown", "Triceps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000010"),
                columns: new[] { "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { 40.0, "Leg Press", "Quadriceps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000011"),
                columns: new[] { "DefaultBeginnerWeightKg", "Name" },
                values: new object[] { 25.0, "Lat Pulldown" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000012"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { "Isolation", 10.0, "Cable Fly", "Chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000013"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { "Bodyweight", 0.0, "Plank", "Core" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000014"),
                columns: new[] { "Name", "TargetMuscle" },
                values: new object[] { "Romanian Deadlift", "Hamstrings" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000015"),
                columns: new[] { "Category", "DefaultBeginnerWeightKg", "Name", "TargetMuscle" },
                values: new object[] { "Isolation", 5.0, "Lateral Raise", "Shoulders" });
        }
    }
}
