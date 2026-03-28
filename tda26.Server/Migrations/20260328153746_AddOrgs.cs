using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_ShopItems_EquippedAvatarUuid",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_ShopItems_EquippedBadgeUuid",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_ShopItems_EquippedBannerUuid",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_ShopItems_EquippedEffectUuid",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_ShopItems_EquippedTitleUuid",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "AccountShopItem");

            migrationBuilder.DropTable(
                name: "DailyRewardTaskStates");

            migrationBuilder.DropTable(
                name: "ShopItems");

            migrationBuilder.DropTable(
                name: "DailyRewardDays");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_EquippedAvatarUuid",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_EquippedBadgeUuid",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_EquippedBannerUuid",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_EquippedEffectUuid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Ducks",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EquippedAvatarUuid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EquippedBadgeUuid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EquippedBannerUuid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EquippedEffectUuid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_Bio",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_FirstName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_LastName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_MiddleName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Student_PictureUrl",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Xp",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "EquippedTitleUuid",
                table: "Accounts",
                newName: "OrganizationUuid");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_EquippedTitleUuid",
                table: "Accounts",
                newName: "IX_Accounts_OrganizationUuid");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationUuid",
                table: "Courses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DisplayName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Uuid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_OrganizationUuid",
                table: "Courses",
                column: "OrganizationUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Organizations_OrganizationUuid",
                table: "Accounts",
                column: "OrganizationUuid",
                principalTable: "Organizations",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Organizations_OrganizationUuid",
                table: "Courses",
                column: "OrganizationUuid",
                principalTable: "Organizations",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Organizations_OrganizationUuid",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Organizations_OrganizationUuid",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Courses_OrganizationUuid",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "OrganizationUuid",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "OrganizationUuid",
                table: "Accounts",
                newName: "EquippedTitleUuid");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_OrganizationUuid",
                table: "Accounts",
                newName: "IX_Accounts_EquippedTitleUuid");

            migrationBuilder.AddColumn<int>(
                name: "Ducks",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EquippedAvatarUuid",
                table: "Accounts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EquippedBadgeUuid",
                table: "Accounts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EquippedBannerUuid",
                table: "Accounts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EquippedEffectUuid",
                table: "Accounts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Student_Bio",
                table: "Accounts",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Student_FirstName",
                table: "Accounts",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Student_LastName",
                table: "Accounts",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Student_MiddleName",
                table: "Accounts",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Student_PictureUrl",
                table: "Accounts",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Xp",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DailyRewardDays",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RewardDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyRewardDays", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_DailyRewardDays_Accounts_AccountUuid",
                        column: x => x.AccountUuid,
                        principalTable: "Accounts",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ShopItems",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Discriminator = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PriceInDucks = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopItems", x => x.Uuid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DailyRewardTaskStates",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DailyRewardDayUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrentValue = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    TaskCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyRewardTaskStates", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_DailyRewardTaskStates_DailyRewardDays_DailyRewardDayUuid",
                        column: x => x.DailyRewardDayUuid,
                        principalTable: "DailyRewardDays",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountShopItem",
                columns: table => new
                {
                    OwnedByAccountsUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ShopItemsUuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountShopItem", x => new { x.OwnedByAccountsUuid, x.ShopItemsUuid });
                    table.ForeignKey(
                        name: "FK_AccountShopItem_Accounts_OwnedByAccountsUuid",
                        column: x => x.OwnedByAccountsUuid,
                        principalTable: "Accounts",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountShopItem_ShopItems_ShopItemsUuid",
                        column: x => x.ShopItemsUuid,
                        principalTable: "ShopItems",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_EquippedAvatarUuid",
                table: "Accounts",
                column: "EquippedAvatarUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_EquippedBadgeUuid",
                table: "Accounts",
                column: "EquippedBadgeUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_EquippedBannerUuid",
                table: "Accounts",
                column: "EquippedBannerUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_EquippedEffectUuid",
                table: "Accounts",
                column: "EquippedEffectUuid");

            migrationBuilder.CreateIndex(
                name: "IX_AccountShopItem_ShopItemsUuid",
                table: "AccountShopItem",
                column: "ShopItemsUuid");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRewardDays_AccountUuid_RewardDate",
                table: "DailyRewardDays",
                columns: new[] { "AccountUuid", "RewardDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyRewardTaskStates_DailyRewardDayUuid_TaskCode",
                table: "DailyRewardTaskStates",
                columns: new[] { "DailyRewardDayUuid", "TaskCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_ShopItems_EquippedAvatarUuid",
                table: "Accounts",
                column: "EquippedAvatarUuid",
                principalTable: "ShopItems",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_ShopItems_EquippedBadgeUuid",
                table: "Accounts",
                column: "EquippedBadgeUuid",
                principalTable: "ShopItems",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_ShopItems_EquippedBannerUuid",
                table: "Accounts",
                column: "EquippedBannerUuid",
                principalTable: "ShopItems",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_ShopItems_EquippedEffectUuid",
                table: "Accounts",
                column: "EquippedEffectUuid",
                principalTable: "ShopItems",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_ShopItems_EquippedTitleUuid",
                table: "Accounts",
                column: "EquippedTitleUuid",
                principalTable: "ShopItems",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
