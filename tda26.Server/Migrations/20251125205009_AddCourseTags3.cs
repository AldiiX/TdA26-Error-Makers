using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTags3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseTag_Tag_TagsUuid",
                table: "CourseTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseTag_Tags_TagsUuid",
                table: "CourseTag",
                column: "TagsUuid",
                principalTable: "Tags",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseTag_Tags_TagsUuid",
                table: "CourseTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseTag_Tag_TagsUuid",
                table: "CourseTag",
                column: "TagsUuid",
                principalTable: "Tag",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
