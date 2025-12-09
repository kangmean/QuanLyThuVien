using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyThuVien.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_AspNetUsers_UserId",
                table: "Rating");

            migrationBuilder.DropForeignKey(
                name: "FK_Rating_Documents_DocumentId",
                table: "Rating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rating",
                table: "Rating");

            migrationBuilder.RenameTable(
                name: "Rating",
                newName: "Ratings");

            migrationBuilder.RenameIndex(
                name: "IX_Rating_UserId",
                table: "Ratings",
                newName: "IX_Ratings_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Rating_DocumentId",
                table: "Ratings",
                newName: "IX_Ratings_DocumentId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Documents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageRating",
                table: "Documents",
                type: "decimal(3,1)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ratings",
                table: "Ratings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_AspNetUsers_UserId",
                table: "Ratings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Documents_DocumentId",
                table: "Ratings",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_AspNetUsers_UserId",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Documents_DocumentId",
                table: "Ratings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ratings",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Documents");

            migrationBuilder.RenameTable(
                name: "Ratings",
                newName: "Rating");

            migrationBuilder.RenameIndex(
                name: "IX_Ratings_UserId",
                table: "Rating",
                newName: "IX_Rating_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Ratings_DocumentId",
                table: "Rating",
                newName: "IX_Rating_DocumentId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Documents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<double>(
                name: "AverageRating",
                table: "Documents",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rating",
                table: "Rating",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_AspNetUsers_UserId",
                table: "Rating",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_Documents_DocumentId",
                table: "Rating",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }
    }
}
