using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExternalAccountConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserExternalAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false),
                    Handle = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExternalNumericId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    VerificationToken = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    TokenExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExternalAccounts", x => x.Id);
                    table.CheckConstraint("CK_UserExternalAccounts_Handle_NotEmpty", "length(trim(Handle)) > 0");
                    table.ForeignKey(
                        name: "FK_UserExternalAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalAccounts_Platform_ExternalNumericId",
                table: "UserExternalAccounts",
                columns: new[] { "Platform", "ExternalNumericId" },
                unique: true,
                filter: "ExternalNumericId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalAccounts_Platform_Handle",
                table: "UserExternalAccounts",
                columns: new[] { "Platform", "Handle" },
                unique: true,
                filter: "Status = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalAccounts_UserId_Platform",
                table: "UserExternalAccounts",
                columns: new[] { "UserId", "Platform" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExternalAccounts");
        }
    }
}
