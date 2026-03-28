using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AccountOrganizationForStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationLecturers");

            migrationBuilder.DropTable(
                name: "OrganizationStudents");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationUuid",
                table: "Courses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationUuid",
                table: "Accounts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_OrganizationUuid",
                table: "Courses",
                column: "OrganizationUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OrganizationUuid",
                table: "Accounts",
                column: "OrganizationUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Organizations_OrganizationUuid",
                table: "Accounts",
                column: "OrganizationUuid",
                principalTable: "Organizations",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Organizations_OrganizationUuid",
                table: "Courses",
                column: "OrganizationUuid",
                principalTable: "Organizations",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Organizations_OrganizationUuid",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Organizations_OrganizationUuid",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_OrganizationUuid",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_OrganizationUuid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "OrganizationUuid",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "OrganizationUuid",
                table: "Accounts");

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
                column: "LecturerUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationStudents_StudentUuid",
                table: "OrganizationStudents",
                column: "StudentUuid",
                unique: true);
        }
    }
}
