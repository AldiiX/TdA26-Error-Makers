using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class MoveIsPremiumToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @has_student_is_premium = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Accounts'
                      AND COLUMN_NAME = 'Student_IsPremium'
                );
            ");

            migrationBuilder.Sql(@"
                SET @sync_sql = IF(
                    @has_student_is_premium > 0,
                    'UPDATE Accounts SET IsPremium = IFNULL(Student_IsPremium, IFNULL(IsPremium, 0));',
                    'UPDATE Accounts SET IsPremium = IFNULL(IsPremium, 0);'
                );
            ");

            migrationBuilder.Sql("PREPARE sync_stmt FROM @sync_sql;");
            migrationBuilder.Sql("EXECUTE sync_stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE sync_stmt;");

            migrationBuilder.Sql(@"
                UPDATE Accounts
                SET IsPremium = 0
                WHERE IsPremium IS NULL;
            ");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPremium",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.Sql(@"
                SET @drop_student_col_sql = IF(
                    (
                        SELECT COUNT(*)
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = DATABASE()
                          AND TABLE_NAME = 'Accounts'
                          AND COLUMN_NAME = 'Student_IsPremium'
                    ) > 0,
                    'ALTER TABLE `Accounts` DROP COLUMN `Student_IsPremium`;',
                    'SELECT 1;'
                );
            ");

            migrationBuilder.Sql("PREPARE drop_student_col_stmt FROM @drop_student_col_sql;");
            migrationBuilder.Sql("EXECUTE drop_student_col_stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE drop_student_col_stmt;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @add_student_col_sql = IF(
                    (
                        SELECT COUNT(*)
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = DATABASE()
                          AND TABLE_NAME = 'Accounts'
                          AND COLUMN_NAME = 'Student_IsPremium'
                    ) = 0,
                    'ALTER TABLE `Accounts` ADD COLUMN `Student_IsPremium` tinyint(1) NULL;',
                    'SELECT 1;'
                );
            ");

            migrationBuilder.Sql("PREPARE add_student_col_stmt FROM @add_student_col_sql;");
            migrationBuilder.Sql("EXECUTE add_student_col_stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE add_student_col_stmt;");

            migrationBuilder.Sql(@"
                UPDATE Accounts
                SET Student_IsPremium = IsPremium
                WHERE Discriminator = 'Student';
            ");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPremium",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");
        }
    }
}
