using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class QuizzesTableCreationsAndUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuestionCount",
                table: "Quizzes",
                newName: "AttemptsCount");

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuizUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Text = table.Column<string>(type: "varchar(1048)", maxLength: 1048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "varchar(34)", maxLength: 34, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_Questions_Quizzes_QuizUuid",
                        column: x => x.QuizUuid,
                        principalTable: "Quizzes",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuizResults",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuizUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizResults", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_QuizResults_Quizzes_QuizUuid",
                        column: x => x.QuizUuid,
                        principalTable: "Quizzes",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuestionUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_QuestionOptions_Questions_QuestionUuid",
                        column: x => x.QuestionUuid,
                        principalTable: "Questions",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuizAnswers",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuizResultUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuestionUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAnswers", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_QuizAnswers_Questions_QuestionUuid",
                        column: x => x.QuestionUuid,
                        principalTable: "Questions",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAnswers_QuizResults_QuizResultUuid",
                        column: x => x.QuizResultUuid,
                        principalTable: "QuizResults",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuizAnswerOptions",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AnswerUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OptionUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAnswerOptions", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_QuizAnswerOptions_QuestionOptions_OptionUuid",
                        column: x => x.OptionUuid,
                        principalTable: "QuestionOptions",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAnswerOptions_QuizAnswers_AnswerUuid",
                        column: x => x.AnswerUuid,
                        principalTable: "QuizAnswers",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionUuid",
                table: "QuestionOptions",
                column: "QuestionUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizUuid",
                table: "Questions",
                column: "QuizUuid");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswerOptions_AnswerUuid",
                table: "QuizAnswerOptions",
                column: "AnswerUuid");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswerOptions_OptionUuid",
                table: "QuizAnswerOptions",
                column: "OptionUuid");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuestionUuid",
                table: "QuizAnswers",
                column: "QuestionUuid");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuizResultUuid",
                table: "QuizAnswers",
                column: "QuizResultUuid");

            migrationBuilder.CreateIndex(
                name: "IX_QuizResults_QuizUuid",
                table: "QuizResults",
                column: "QuizUuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizAnswerOptions");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "QuizAnswers");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "QuizResults");

            migrationBuilder.RenameColumn(
                name: "AttemptsCount",
                table: "Quizzes",
                newName: "QuestionCount");
        }
    }
}
