using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserExternalAccountAndAddFilteredUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserExternalAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false),
                    Handle = table.Column<string>(type: "TEXT", nullable: false),
                    ExternalNumericId = table.Column<string>(type: "TEXT", nullable: true),
                    VerificationToken = table.Column<string>(type: "TEXT", nullable: true),
                    TokenExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExternalAccount", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalAccount_Platform_ExternalNumericId",
                table: "UserExternalAccount",
                columns: new[] { "Platform", "ExternalNumericId" },
                unique: true,
                filter: "[ExternalNumericId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalAccount_Platform_Handle",
                table: "UserExternalAccount",
                columns: new[] { "Platform", "Handle" },
                unique: true,
                filter: "[Status] = 'Verified'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExternalAccount");
        }
    }
}
