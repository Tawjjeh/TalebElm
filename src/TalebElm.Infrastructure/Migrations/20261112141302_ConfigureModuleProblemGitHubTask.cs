using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureModuleProblemGitHubTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GitHubTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateRepository = table.Column<string>(type: "TEXT", nullable: false),
                    WorkflowFileName = table.Column<string>(type: "TEXT", nullable: false),
                    MinPassingTests = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubTask", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleProblem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalIdentifier = table.Column<string>(type: "TEXT", nullable: false),
                    GitHubTaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleProblem", x => x.Id);
                    table.CheckConstraint("CK_ModuleProblems_GitHubTaskId_Value", "(Kind = 1 AND GitHubTaskId IS NULL) OR (Kind = 2 AND GitHubTaskId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ModuleProblem_GitHubTask_GitHubTaskId",
                        column: x => x.GitHubTaskId,
                        principalTable: "GitHubTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModuleProblem_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleProblem_GitHubTaskId",
                table: "ModuleProblem",
                column: "GitHubTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleProblem_ModuleId_Order",
                table: "ModuleProblem",
                columns: new[] { "ModuleId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleProblem");

            migrationBuilder.DropTable(
                name: "GitHubTask");
        }
    }
}
