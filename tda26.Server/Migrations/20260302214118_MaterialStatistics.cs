using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tda26.Server.Migrations
{
    /// <inheritdoc />
    public partial class MaterialStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastClickedAt",
                table: "Materials",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDownload",
                table: "Materials",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalBytesDownloaded",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Materials",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "LastClickedAt",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "LastDownload",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "TotalBytesDownloaded",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Materials");
        }
    }
}
