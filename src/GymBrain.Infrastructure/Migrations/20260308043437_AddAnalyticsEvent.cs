using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventName = table.Column<string>(type: "text", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "Category", "CreatedAtUtc", "DefaultBeginnerWeightKg", "Equipment", "Name", "TargetMuscle", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000066"), "Warmup", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "cardio", "Treadmill Walk", "Full Body", null },
                    { new Guid("10000001-0000-0000-0000-000000000067"), "Warmup", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "cardio", "Stationary Bike", "Full Body", null },
                    { new Guid("10000001-0000-0000-0000-000000000068"), "Warmup", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Jumping Jacks", "Full Body", null },
                    { new Guid("10000001-0000-0000-0000-000000000069"), "Warmup", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Arm Circles", "Shoulders", null },
                    { new Guid("10000001-0000-0000-0000-000000000070"), "Warmup", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, "bodyweight", "Bodyweight Air Squats", "Legs", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEvents");

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000066"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000067"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000068"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000069"));

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000070"));
        }
    }
}
