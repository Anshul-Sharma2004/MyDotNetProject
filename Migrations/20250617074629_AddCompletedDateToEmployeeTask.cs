using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleBasedJWTMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedDateToEmployeeTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "EmployeeTasks",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "EmployeeTasks");
        }
    }
}
