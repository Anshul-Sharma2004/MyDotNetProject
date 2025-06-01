using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleBasedJWTMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecializationToTeamAssign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "TeamAssigns",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "TeamAssigns");
        }
    }
}
