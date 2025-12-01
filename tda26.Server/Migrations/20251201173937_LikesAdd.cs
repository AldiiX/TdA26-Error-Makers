using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class LikesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionCount",
                table: "Quizzes");

            migrationBuilder.AddColumn<Guid>(
                name: "LecturerUuid",
                table: "Courses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_LecturerUuid",
                table: "Courses",
                column: "LecturerUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Accounts_LecturerUuid",
                table: "Courses",
                column: "LecturerUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Accounts_LecturerUuid",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_LecturerUuid",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LecturerUuid",
                table: "Courses");

            migrationBuilder.AddColumn<int>(
                name: "QuestionCount",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
