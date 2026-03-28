using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Student_Bio",
                table: "Accounts",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Student_FirstName",
                table: "Accounts",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Student_IsPremium",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Student_LastName",
                table: "Accounts",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Student_MiddleName",
                table: "Accounts",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Student_PictureUrl",
                table: "Accounts",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Student_Bio",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_FirstName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_IsPremium",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_LastName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_MiddleName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_PictureUrl",
                table: "Accounts");
        }
    }
}
