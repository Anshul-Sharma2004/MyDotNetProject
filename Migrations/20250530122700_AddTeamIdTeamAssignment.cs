using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleBasedJWTMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamIdTeamAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "TaskAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "TaskAssignments");
        }
    }
}
