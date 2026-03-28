using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Student_Bio",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_FirstName",
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

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DisplayName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Uuid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrganizationLecturers",
                columns: table => new
                {
                    OrganizationUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LecturerUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationLecturers", x => new { x.OrganizationUuid, x.LecturerUuid });
                    table.ForeignKey(
                        name: "FK_OrganizationLecturers_Accounts_LecturerUuid",
                        column: x => x.LecturerUuid,
                        principalTable: "Accounts",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationLecturers_Organizations_OrganizationUuid",
                        column: x => x.OrganizationUuid,
                        principalTable: "Organizations",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrganizationStudents",
                columns: table => new
                {
                    OrganizationUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StudentUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationStudents", x => new { x.OrganizationUuid, x.StudentUuid });
                    table.ForeignKey(
                        name: "FK_OrganizationStudents_Accounts_StudentUuid",
                        column: x => x.StudentUuid,
                        principalTable: "Accounts",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationStudents_Organizations_OrganizationUuid",
                        column: x => x.OrganizationUuid,
                        principalTable: "Organizations",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLecturers_LecturerUuid",
                table: "OrganizationLecturers",
                column: "LecturerUuid");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationStudents_StudentUuid",
                table: "OrganizationStudents",
                column: "StudentUuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationLecturers");

            migrationBuilder.DropTable(
                name: "OrganizationStudents");

            migrationBuilder.DropTable(
                name: "Organizations");

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
    }
}
