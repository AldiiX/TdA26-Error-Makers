using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class Mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopItems_Accounts_StudentUuid",
                table: "ShopItems");

            migrationBuilder.RenameColumn(
                name: "StudentUuid",
                table: "ShopItems",
                newName: "AccountUuid");

            migrationBuilder.RenameIndex(
                name: "IX_ShopItems_StudentUuid",
                table: "ShopItems",
                newName: "IX_ShopItems_AccountUuid");

            migrationBuilder.AddColumn<string>(
                name: "AvatarShopItem_ImageUrl",
                table: "ShopItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ShopItems",
                type: "varchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ShopItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopItems_Accounts_AccountUuid",
                table: "ShopItems",
                column: "AccountUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopItems_Accounts_AccountUuid",
                table: "ShopItems");

            migrationBuilder.DropColumn(
                name: "AvatarShopItem_ImageUrl",
                table: "ShopItems");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ShopItems");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ShopItems");

            migrationBuilder.RenameColumn(
                name: "AccountUuid",
                table: "ShopItems",
                newName: "StudentUuid");

            migrationBuilder.RenameIndex(
                name: "IX_ShopItems_AccountUuid",
                table: "ShopItems",
                newName: "IX_ShopItems_StudentUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopItems_Accounts_StudentUuid",
                table: "ShopItems",
                column: "StudentUuid",
                principalTable: "Accounts",
                principalColumn: "Uuid");
        }
    }
}
