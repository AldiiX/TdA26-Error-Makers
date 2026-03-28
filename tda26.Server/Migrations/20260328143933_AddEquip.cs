using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEquip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<Guid>(
                name: "EquippedTitleUuid",
                table: "Accounts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

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
                name: "IX_Accounts_EquippedTitleUuid",
                table: "Accounts",
                column: "EquippedTitleUuid");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Accounts_EquippedTitleUuid",
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
                name: "EquippedTitleUuid",
                table: "Accounts");
        }
    }
}
