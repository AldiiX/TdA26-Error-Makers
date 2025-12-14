using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tags",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryUuid",
                table: "Tags",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryUuid",
                table: "Courses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Label = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Uuid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CategoryUuid",
                table: "Tags",
                column: "CategoryUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DisplayName",
                table: "Tags",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CategoryUuid",
                table: "Courses",
                column: "CategoryUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Categories_CategoryUuid",
                table: "Courses",
                column: "CategoryUuid",
                principalTable: "Categories",
                principalColumn: "Uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Categories_CategoryUuid",
                table: "Tags",
                column: "CategoryUuid",
                principalTable: "Categories",
                principalColumn: "Uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Categories_CategoryUuid",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Categories_CategoryUuid",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Tags_CategoryUuid",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_DisplayName",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CategoryUuid",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CategoryUuid",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "CategoryUuid",
                table: "Courses");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tags",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
