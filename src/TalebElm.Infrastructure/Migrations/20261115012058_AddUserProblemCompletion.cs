using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProblemCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProblemCompletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleProblemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalSubmissionId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DetailsJson = table.Column<string>(type: "TEXT", nullable: true),
                    VerifiedHeadCommitSha = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    TaskVersionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProblemCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProblemCompletions_ModuleProblem_ModuleProblemId",
                        column: x => x.ModuleProblemId,
                        principalTable: "ModuleProblem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProblemCompletions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProblemCompletions_ModuleProblemId",
                table: "UserProblemCompletions",
                column: "ModuleProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProblemCompletions_UserId_ModuleProblemId",
                table: "UserProblemCompletions",
                columns: new[] { "UserId", "ModuleProblemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProblemCompletions");
        }
    }
}
