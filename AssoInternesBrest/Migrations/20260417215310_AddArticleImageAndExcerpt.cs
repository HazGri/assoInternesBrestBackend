using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssoInternesBrest.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleImageAndExcerpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Excerpt",
                table: "Articles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Articles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ImageId",
                table: "Articles",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Images_ImageId",
                table: "Articles",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Images_ImageId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_ImageId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Excerpt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Articles");
        }
    }
}
