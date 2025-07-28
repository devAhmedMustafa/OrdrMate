using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class WorkingHoursAndDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndWorkingHour",
                table: "Branch",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartWorkingHour",
                table: "Branch",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool[]>(
                name: "WorkingDays",
                table: "Branch",
                type: "boolean[]",
                nullable: false,
                defaultValue: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndWorkingHour",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "StartWorkingHour",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Branch");
        }
    }
}
