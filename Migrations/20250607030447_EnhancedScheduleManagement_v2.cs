using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeScheduling.API.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedScheduleManagement_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 7, 3, 4, 47, 242, DateTimeKind.Utc).AddTicks(8543));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 7, 1, 45, 2, 810, DateTimeKind.Utc).AddTicks(6378));
        }
    }
}
