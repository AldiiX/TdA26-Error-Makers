using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DisplayName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Uuid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.DropTable(
                name: "Tag");
        }
    }
}
