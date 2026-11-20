using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureGitHubTaskAndTaskVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GitHubTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionTag = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CommitSha = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    WorkflowFileSha = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    TestTreeSha = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskVersions_GitHubTask_GitHubTaskId",
                        column: x => x.GitHubTaskId,
                        principalTable: "GitHubTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_GitHubTask_MinPassingTests_Positive",
                table: "GitHubTask",
                sql: "MinPassingTests IS NULL OR MinPassingTests > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GitHubTask_TemplateRepository_NotEmpty",
                table: "GitHubTask",
                sql: "length(trim(TemplateRepository)) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaskVersions_GitHubTaskId",
                table: "TaskVersions",
                column: "GitHubTaskId",
                unique: true,
                filter: "Status = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TaskVersions_GitHubTaskId_VersionTag",
                table: "TaskVersions",
                columns: new[] { "GitHubTaskId", "VersionTag" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GitHubTask_MinPassingTests_Positive",
                table: "GitHubTask");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GitHubTask_TemplateRepository_NotEmpty",
                table: "GitHubTask");
        }
    }
}
