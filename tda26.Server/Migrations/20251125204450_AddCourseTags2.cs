using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTags2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Courses_CourseUuid",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_CourseUuid",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "CourseUuid",
                table: "Tag");

            migrationBuilder.CreateTable(
                name: "CourseTag",
                columns: table => new
                {
                    CoursesUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TagsUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTag", x => new { x.CoursesUuid, x.TagsUuid });
                    table.ForeignKey(
                        name: "FK_CourseTag_Courses_CoursesUuid",
                        column: x => x.CoursesUuid,
                        principalTable: "Courses",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseTag_Tag_TagsUuid",
                        column: x => x.TagsUuid,
                        principalTable: "Tag",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CourseTag_TagsUuid",
                table: "CourseTag",
                column: "TagsUuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseTag");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseUuid",
                table: "Tag",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_CourseUuid",
                table: "Tag",
                column: "CourseUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Courses_CourseUuid",
                table: "Tag",
                column: "CourseUuid",
                principalTable: "Courses",
                principalColumn: "Uuid");
        }
    }
}
