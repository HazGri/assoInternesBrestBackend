using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssoInternesBrest.Migrations
{
    /// <inheritdoc />
    public partial class AddBureauMemberImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "BureauMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BureauMembers_ImageId",
                table: "BureauMembers",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_BureauMembers_Images_ImageId",
                table: "BureauMembers",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BureauMembers_Images_ImageId",
                table: "BureauMembers");

            migrationBuilder.DropIndex(
                name: "IX_BureauMembers_ImageId",
                table: "BureauMembers");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "BureauMembers");
        }
    }
}
