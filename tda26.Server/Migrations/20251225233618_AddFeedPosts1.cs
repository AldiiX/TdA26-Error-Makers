using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedPosts1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedItems_Courses_CourseUuid",
                table: "FeedItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeedItems",
                table: "FeedItems");

            migrationBuilder.RenameTable(
                name: "FeedItems",
                newName: "FeedPosts");

            migrationBuilder.RenameIndex(
                name: "IX_FeedItems_CourseUuid",
                table: "FeedPosts",
                newName: "IX_FeedPosts_CourseUuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountUuid",
                table: "FeedPosts",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "Edited",
                table: "FeedPosts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "FeedPosts",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "FeedPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeedPosts",
                table: "FeedPosts",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IX_FeedPosts_AccountUuid",
                table: "FeedPosts",
                column: "AccountUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedPosts_Accounts_AccountUuid",
                table: "FeedPosts",
                column: "AccountUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedPosts_Courses_CourseUuid",
                table: "FeedPosts",
                column: "CourseUuid",
                principalTable: "Courses",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedPosts_Accounts_AccountUuid",
                table: "FeedPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedPosts_Courses_CourseUuid",
                table: "FeedPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeedPosts",
                table: "FeedPosts");

            migrationBuilder.DropIndex(
                name: "IX_FeedPosts_AccountUuid",
                table: "FeedPosts");

            migrationBuilder.DropColumn(
                name: "AccountUuid",
                table: "FeedPosts");

            migrationBuilder.DropColumn(
                name: "Edited",
                table: "FeedPosts");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "FeedPosts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "FeedPosts");

            migrationBuilder.RenameTable(
                name: "FeedPosts",
                newName: "FeedItems");

            migrationBuilder.RenameIndex(
                name: "IX_FeedPosts_CourseUuid",
                table: "FeedItems",
                newName: "IX_FeedItems_CourseUuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeedItems",
                table: "FeedItems",
                column: "Uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedItems_Courses_CourseUuid",
                table: "FeedItems",
                column: "CourseUuid",
                principalTable: "Courses",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
