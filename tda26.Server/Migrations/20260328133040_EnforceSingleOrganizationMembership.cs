using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleOrganizationMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationStudents_Accounts_StudentUuid",
                table: "OrganizationStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLecturers_Accounts_LecturerUuid",
                table: "OrganizationLecturers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationStudents_StudentUuid",
                table: "OrganizationStudents");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationLecturers_LecturerUuid",
                table: "OrganizationLecturers");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationStudents_StudentUuid",
                table: "OrganizationStudents",
                column: "StudentUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLecturers_LecturerUuid",
                table: "OrganizationLecturers",
                column: "LecturerUuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationStudents_Accounts_StudentUuid",
                table: "OrganizationStudents",
                column: "StudentUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLecturers_Accounts_LecturerUuid",
                table: "OrganizationLecturers",
                column: "LecturerUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationStudents_Accounts_StudentUuid",
                table: "OrganizationStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLecturers_Accounts_LecturerUuid",
                table: "OrganizationLecturers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationStudents_StudentUuid",
                table: "OrganizationStudents");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationLecturers_LecturerUuid",
                table: "OrganizationLecturers");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationStudents_StudentUuid",
                table: "OrganizationStudents",
                column: "StudentUuid");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLecturers_LecturerUuid",
                table: "OrganizationLecturers",
                column: "LecturerUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationStudents_Accounts_StudentUuid",
                table: "OrganizationStudents",
                column: "StudentUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLecturers_Accounts_LecturerUuid",
                table: "OrganizationLecturers",
                column: "LecturerUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
