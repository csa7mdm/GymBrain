using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutsCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkoutsCompleted",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkoutsCompleted",
                table: "Users");
        }
    }
}
