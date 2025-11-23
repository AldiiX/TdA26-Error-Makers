using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddLecturerIsPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Accounts");
        }
    }
}
