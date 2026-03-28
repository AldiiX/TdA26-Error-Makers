using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopItems_Accounts_AccountUuid",
                table: "ShopItems");

            migrationBuilder.DropIndex(
                name: "IX_ShopItems_AccountUuid",
                table: "ShopItems");

            migrationBuilder.DropColumn(
                name: "AccountUuid",
                table: "ShopItems");

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
                name: "IX_AccountShopItem_ShopItemsUuid",
                table: "AccountShopItem",
                column: "ShopItemsUuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountShopItem");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountUuid",
                table: "ShopItems",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ShopItems_AccountUuid",
                table: "ShopItems",
                column: "AccountUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopItems_Accounts_AccountUuid",
                table: "ShopItems",
                column: "AccountUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid");
        }
    }
}
