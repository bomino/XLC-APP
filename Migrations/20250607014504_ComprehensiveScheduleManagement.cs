using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeScheduling.API.Migrations
{
    /// <inheritdoc />
    public partial class ComprehensiveScheduleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "ConfirmedAt",
                table: "Assignments",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Shifts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Shifts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AvailabilityType",
                table: "Availabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Availabilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Availabilities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Availabilities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "Assignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime",
                table: "Assignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "HoursWorked",
                table: "Assignments",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Assignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeHours",
                table: "Assignments",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RegularHours",
                table: "Assignments",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPay",
                table: "Assignments",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 7, 1, 45, 2, 810, DateTimeKind.Utc).AddTicks(6378));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "AvailabilityType",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "HoursWorked",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "RegularHours",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "TotalPay",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Assignments",
                newName: "ConfirmedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Availabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Assignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 6, 11, 24, 28, 637, DateTimeKind.Utc).AddTicks(6213));
        }
    }
}
