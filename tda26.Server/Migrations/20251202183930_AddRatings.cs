using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropColumn(
                name: "QuestionCount",
                table: "Quizzes");

            migrationBuilder.AddColumn<Guid>(
                name: "LecturerUuid",
                table: "Courses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CourseUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Discriminator = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_Ratings_Accounts_AccountUuid",
                        column: x => x.AccountUuid,
                        principalTable: "Accounts",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ratings_Courses_CourseUuid",
                        column: x => x.CourseUuid,
                        principalTable: "Courses",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_LecturerUuid",
                table: "Courses",
                column: "LecturerUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_AccountUuid_CourseUuid",
                table: "Ratings",
                columns: new[] { "AccountUuid", "CourseUuid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_CourseUuid",
                table: "Ratings",
                column: "CourseUuid");

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

            migrationBuilder.DropTable(
                name: "Ratings");

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

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CourseUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_Likes_Accounts_AccountUuid",
                        column: x => x.AccountUuid,
                        principalTable: "Accounts",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Courses_CourseUuid",
                        column: x => x.CourseUuid,
                        principalTable: "Courses",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_AccountUuid_CourseUuid",
                table: "Likes",
                columns: new[] { "AccountUuid", "CourseUuid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_CourseUuid",
                table: "Likes",
                column: "CourseUuid");
        }
    }
}
