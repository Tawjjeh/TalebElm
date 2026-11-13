using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatesInProjectMilestone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapstoneProject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapstoneProject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapstoneProject_GitHubTask_TaskId",
                        column: x => x.TaskId,
                        principalTable: "GitHubTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapstoneProject_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMilestone",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkflowJobName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMilestone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMilestone_CapstoneProject_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "CapstoneProject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMilestone_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProject_TaskId",
                table: "CapstoneProject",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProject_TrackId",
                table: "CapstoneProject",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestone_ModuleId",
                table: "ProjectMilestone",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestone_ProjectId_Order",
                table: "ProjectMilestone",
                columns: new[] { "ProjectId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestone_ProjectId_WorkflowJobName",
                table: "ProjectMilestone",
                columns: new[] { "ProjectId", "WorkflowJobName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMilestone");

            migrationBuilder.DropTable(
                name: "CapstoneProject");
        }
    }
}
