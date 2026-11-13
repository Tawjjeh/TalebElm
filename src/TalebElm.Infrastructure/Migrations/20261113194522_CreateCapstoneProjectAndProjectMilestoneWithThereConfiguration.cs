using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCapstoneProjectAndProjectMilestoneWithThereConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapstoneProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapstoneProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapstoneProjects_GitHubTask_TaskId",
                        column: x => x.TaskId,
                        principalTable: "GitHubTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapstoneProjects_GitHubTask_TaskId1",
                        column: x => x.TaskId1,
                        principalTable: "GitHubTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapstoneProjects_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapstoneProjects_Tracks_TrackId1",
                        column: x => x.TrackId1,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkflowJobName = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectId1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId1 = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_CapstoneProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "CapstoneProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_CapstoneProjects_ProjectId1",
                        column: x => x.ProjectId1,
                        principalTable: "CapstoneProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_Modules_ModuleId1",
                        column: x => x.ModuleId1,
                        principalTable: "Modules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProjects_TaskId",
                table: "CapstoneProjects",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProjects_TaskId1",
                table: "CapstoneProjects",
                column: "TaskId1");

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProjects_TrackId",
                table: "CapstoneProjects",
                column: "TrackId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProjects_TrackId1",
                table: "CapstoneProjects",
                column: "TrackId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_ModuleId",
                table: "ProjectMilestones",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_ModuleId1",
                table: "ProjectMilestones",
                column: "ModuleId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_ProjectId_Order",
                table: "ProjectMilestones",
                columns: new[] { "ProjectId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_ProjectId1",
                table: "ProjectMilestones",
                column: "ProjectId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMilestones");

            migrationBuilder.DropTable(
                name: "CapstoneProjects");
        }
    }
}
