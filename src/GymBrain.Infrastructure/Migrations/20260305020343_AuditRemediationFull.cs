using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditRemediationFull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyCalories",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DaysPerWeek",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DietaryPreference",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentJson",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Goal",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Injuries",
                table: "Users",
                type: "text",
                nullable: true);

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
                column: "Equipment",
                value: "barbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000002"),
                column: "Equipment",
                value: "barbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000003"),
                column: "Equipment",
                value: "barbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000004"),
                column: "Equipment",
                value: "barbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000005"),
                column: "Equipment",
                value: "barbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000006"),
                columns: new[] { "Category", "Equipment" },
                values: new object[] { "Compound", "bodyweight" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000007"),
                column: "Equipment",
                value: "dumbbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000008"),
                column: "Equipment",
                value: "dumbbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000009"),
                column: "Equipment",
                value: "cable");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000010"),
                column: "Equipment",
                value: "machine");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000011"),
                column: "Equipment",
                value: "cable");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000012"),
                column: "Equipment",
                value: "cable");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000013"),
                column: "Equipment",
                value: "bodyweight");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000014"),
                column: "Equipment",
                value: "barbell");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000015"),
                column: "Equipment",
                value: "dumbbell");

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "Category", "CreatedAtUtc", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000016"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "Incline Barbell Bench Press", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000017"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "dumbbell", "Dumbbell Bench Press", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000018"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Incline Dumbbell Press", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000019"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Pec Deck", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000020"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "machine", "Smith Machine Flat Press", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000021"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Smith Machine Incline Press", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000022"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "cable", "Seated Cable Row", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000023"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "T-Bar Row", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000024"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Straight-Arm Pulldown", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000025"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Back Extension", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000026"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Chin-Up", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000027"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Dumbbell Shoulder Press", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000028"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "cable", "Cable Lateral Raise", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000029"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Face Pull", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000030"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "dumbbell", "Front Raise", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000031"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "machine", "Reverse Pec Deck", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000032"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Arnold Press", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000033"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "cable", "Cable Shrug", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000034"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Leg Extension", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000035"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "machine", "Hack Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000036"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "dumbbell", "Goblet Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000037"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Bulgarian Split Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000038"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Leg Curl", "Hamstrings", null },
                    { new Guid("10000001-0000-0000-0000-000000000039"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "Hip Thrust", "Glutes", null },
                    { new Guid("10000001-0000-0000-0000-000000000040"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Glute Bridge", "Glutes", null },
                    { new Guid("10000001-0000-0000-0000-000000000041"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "cable", "Cable Pull-Through", "Glutes", null },
                    { new Guid("10000001-0000-0000-0000-000000000042"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "barbell", "Barbell Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000043"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6.0, "dumbbell", "Incline Dumbbell Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000044"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "dumbbell", "Hammer Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000045"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Cable Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000046"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8.0, "barbell", "Preacher Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000047"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "barbell", "Skull Crushers", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000048"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, "cable", "Cable Overhead Tricep Extension", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000049"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "dumbbell", "Dumbbell Tricep Kickback", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000050"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "barbell", "Close-Grip Bench Press", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000051"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Dips", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000052"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Hanging Leg Raise", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000053"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Ab Wheel Rollout", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000054"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "dumbbell", "Russian Twist", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000055"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "machine", "Standing Calf Raise", "Calves", null },
                    { new Guid("10000001-0000-0000-0000-000000000056"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "machine", "Seated Calf Raise", "Calves", null },
                    { new Guid("10000001-0000-0000-0000-000000000057"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, "barbell", "Barbell Shrug", "Traps", null },
                    { new Guid("10000001-0000-0000-0000-000000000058"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "dumbbell", "Dumbbell Shrug", "Traps", null },
                    { new Guid("10000001-0000-0000-0000-000000000059"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "barbell", "Wrist Curl", "Forearms", null },
                    { new Guid("10000001-0000-0000-0000-000000000060"), "Isolation", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3.0, "barbell", "Reverse Wrist Curl", "Forearms", null },
                    { new Guid("10000001-0000-0000-0000-000000000061"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, "barbell", "Clean and Press", "Full Body", null },
                    { new Guid("10000001-0000-0000-0000-000000000062"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Burpees", "Full Body", null },
                    { new Guid("10000001-0000-0000-0000-000000000063"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Diamond Push-Up", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000064"), "Plyometric", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Box Jump", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000065"), "Compound", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, "bodyweight", "Medicine Ball Slam", "Full Body", null }
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
                name: "DailyCalories",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DaysPerWeek",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DietaryPreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EquipmentJson",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Injuries",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Exercises");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000006"),
                column: "Category",
                value: "Bodyweight");
        }
    }
}
