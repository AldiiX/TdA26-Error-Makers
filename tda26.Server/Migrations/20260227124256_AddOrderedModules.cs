using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderedModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModuleUuid",
                table: "Quizzes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleUuid",
                table: "Materials",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Materials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CourseModules",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1048)", maxLength: 1048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CourseUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ModuleUuid",
                table: "Quizzes",
                column: "ModuleUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ModuleUuid",
                table: "Materials",
                column: "ModuleUuid");

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_CourseUuid",
                table: "CourseModules",
                column: "CourseUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_CourseModules_ModuleUuid",
                table: "Materials",
                column: "ModuleUuid",
                principalTable: "CourseModules",
                principalColumn: "Uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_CourseModules_ModuleUuid",
                table: "Quizzes",
                column: "ModuleUuid",
                principalTable: "CourseModules",
                principalColumn: "Uuid");
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

            migrationBuilder.DropTable(
                name: "CourseModules");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ModuleUuid",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Materials_ModuleUuid",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ModuleUuid",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ModuleUuid",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Materials");
        }
    }
}
