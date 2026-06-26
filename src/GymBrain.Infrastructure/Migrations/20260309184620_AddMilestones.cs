using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMilestones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ChapterNumber = table.Column<int>(type: "integer", nullable: false),
                    ConditionType = table.Column<string>(type: "text", nullable: false),
                    ConditionValue = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnlockedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMilestones", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Milestones",
                columns: new[] { "Id", "ChapterNumber", "ConditionType", "ConditionValue", "CreatedAtUtc", "Description", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 1, "WorkoutCount", "1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Complete your first workout session.", "The Commitment", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1, "WorkoutCount", "3", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Complete 3 total workouts.", "Building Momentum", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 2, "WorkoutsInWeek", "2", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Complete 2 workouts in a single week.", "Double Down", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 2, "Streak", "3", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Maintain a 3-day workout streak.", "Consistency King", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), 3, "WorkoutCount", "15", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Complete 15 total workouts.", "Veteran", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropTable(
                name: "UserMilestones");
        }
    }
}
