using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleBasedJWTMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamIdToTeamAssign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "TeamAssigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TeamAssigns_TeamId",
                table: "TeamAssigns",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamAssigns_Teams_TeamId",
                table: "TeamAssigns",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamAssigns_Teams_TeamId",
                table: "TeamAssigns");

            migrationBuilder.DropIndex(
                name: "IX_TeamAssigns_TeamId",
                table: "TeamAssigns");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "TeamAssigns");
        }
    }
}
