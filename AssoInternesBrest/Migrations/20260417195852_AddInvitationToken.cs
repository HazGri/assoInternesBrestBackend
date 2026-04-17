using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssoInternesBrest.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvitationToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitationTokenExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InvitationTokenExpiresAt",
                table: "Users");
        }
    }
}
