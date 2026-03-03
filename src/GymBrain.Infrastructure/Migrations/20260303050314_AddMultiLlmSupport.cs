using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiLlmSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LlmProvider",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredModel",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000001"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000002"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000003"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000004"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000005"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000006"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000007"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000008"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000009"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000010"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000011"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000012"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000013"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000014"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000015"),
                column: "CreatedAtUtc",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LlmProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredModel",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000001"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 74, DateTimeKind.Utc).AddTicks(9357));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000002"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(756));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000003"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(760));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000004"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(762));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000005"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(764));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000006"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(766));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000007"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(767));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000008"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(783));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000009"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(785));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000010"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(786));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000011"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(788));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000012"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(790));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000013"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(792));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000014"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(793));

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: new Guid("10000001-0000-0000-0000-000000000015"),
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 3, 4, 13, 31, 75, DateTimeKind.Utc).AddTicks(795));
        }
    }
}
