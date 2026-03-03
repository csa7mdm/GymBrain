using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetMuscle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DefaultBeginnerWeightKg = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    encrypted_api_key = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    TonePersona = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Motivational Coach"),
                    ExperienceLevel = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "Category", "CreatedAtUtc", "DefaultBeginnerWeightKg", "Name", "TargetMuscle", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000001"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 74, DateTimeKind.Utc).AddTicks(9357), 20.0, "Barbell Squat", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000002"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(756), 20.0, "Barbell Deadlift", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000003"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(760), 20.0, "Barbell Bench Press", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000004"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(762), 15.0, "Overhead Press", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000005"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(764), 20.0, "Barbell Row", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000006"), "Bodyweight", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(766), 0.0, "Pull-Up", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000007"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(767), 10.0, "Dumbbell Lunges", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000008"), "Isolation", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(783), 8.0, "Dumbbell Curl", "Biceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000009"), "Isolation", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(785), 10.0, "Tricep Pushdown", "Triceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000010"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(786), 40.0, "Leg Press", "Quadriceps", null },
                    { new Guid("10000001-0000-0000-0000-000000000011"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(788), 25.0, "Lat Pulldown", "Back", null },
                    { new Guid("10000001-0000-0000-0000-000000000012"), "Isolation", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(790), 10.0, "Cable Fly", "Chest", null },
                    { new Guid("10000001-0000-0000-0000-000000000013"), "Bodyweight", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(792), 0.0, "Plank", "Core", null },
                    { new Guid("10000001-0000-0000-0000-000000000014"), "Compound", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(793), 20.0, "Romanian Deadlift", "Hamstrings", null },
                    { new Guid("10000001-0000-0000-0000-000000000015"), "Isolation", new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(795), 5.0, "Lateral Raise", "Shoulders", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
