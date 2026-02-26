using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseModules",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "varchar(1048)", maxLength: 1048, nullable: true),
                    IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CourseUuid = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseModules", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_CourseModules_Courses_CourseUuid",
                        column: x => x.CourseUuid,
                        principalTable: "Courses",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleUuid",
                table: "Quizzes",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleUuid",
                table: "Materials",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_CourseUuid_Order",
                table: "CourseModules",
                columns: new[] { "CourseUuid", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ModuleUuid_Order",
                table: "Materials",
                columns: new[] { "ModuleUuid", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ModuleUuid_Order",
                table: "Quizzes",
                columns: new[] { "ModuleUuid", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_CourseModules_ModuleUuid",
                table: "Materials",
                column: "ModuleUuid",
                principalTable: "CourseModules",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_CourseModules_ModuleUuid",
                table: "Quizzes",
                column: "ModuleUuid",
                principalTable: "CourseModules",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_CourseModules_ModuleUuid",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_CourseModules_ModuleUuid",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Materials_ModuleUuid_Order",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ModuleUuid_Order",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ModuleUuid",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ModuleUuid",
                table: "Materials");

            migrationBuilder.DropTable(
                name: "CourseModules");
        }
    }
}
