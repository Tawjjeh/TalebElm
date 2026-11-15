using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCapstoneProjectAndProjectMilestone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapstoneProject_GitHubTask_TaskId",
                table: "CapstoneProject");

            migrationBuilder.DropForeignKey(
                name: "FK_CapstoneProject_Tracks_TrackId",
                table: "CapstoneProject");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMilestone_CapstoneProject_ProjectId",
                table: "ProjectMilestone");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMilestone_Modules_ModuleId",
                table: "ProjectMilestone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectMilestone",
                table: "ProjectMilestone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CapstoneProject",
                table: "CapstoneProject");

            migrationBuilder.DropIndex(
                name: "IX_CapstoneProject_TaskId",
                table: "CapstoneProject");

            migrationBuilder.DropIndex(
                name: "IX_CapstoneProject_TrackId",
                table: "CapstoneProject");

            migrationBuilder.RenameTable(
                name: "ProjectMilestone",
                newName: "ProjectMilestones");

            migrationBuilder.RenameTable(
                name: "CapstoneProject",
                newName: "CapstoneProjects");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMilestone_ProjectId_WorkflowJobName",
                table: "ProjectMilestones",
                newName: "IX_ProjectMilestones_ProjectId_WorkflowJobName");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMilestone_ProjectId_Order",
                table: "ProjectMilestones",
                newName: "IX_ProjectMilestones_ProjectId_Order");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMilestone_ModuleId",
                table: "ProjectMilestones",
                newName: "IX_ProjectMilestones_ModuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectMilestones",
                table: "ProjectMilestones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CapstoneProjects",
                table: "CapstoneProjects",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectMilestones_Order_Positive",
                table: "ProjectMilestones",
                sql: "\"Order\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectMilestones_Title_NotEmpty",
                table: "ProjectMilestones",
                sql: "length(trim(Title)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectMilestones_WorkflowJobName_NotEmpty",
                table: "ProjectMilestones",
                sql: "length(trim(WorkflowJobName)) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProjects_TaskId",
                table: "CapstoneProjects",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProjects_TrackId",
                table: "CapstoneProjects",
                column: "TrackId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CapstoneProjects_Title_NotEmpty",
                table: "CapstoneProjects",
                sql: "length(trim(Title)) > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_CapstoneProjects_GitHubTask_TaskId",
                table: "CapstoneProjects",
                column: "TaskId",
                principalTable: "GitHubTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CapstoneProjects_Tracks_TrackId",
                table: "CapstoneProjects",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMilestones_CapstoneProjects_ProjectId",
                table: "ProjectMilestones",
                column: "ProjectId",
                principalTable: "CapstoneProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMilestones_Modules_ModuleId",
                table: "ProjectMilestones",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapstoneProjects_GitHubTask_TaskId",
                table: "CapstoneProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_CapstoneProjects_Tracks_TrackId",
                table: "CapstoneProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMilestones_CapstoneProjects_ProjectId",
                table: "ProjectMilestones");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMilestones_Modules_ModuleId",
                table: "ProjectMilestones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectMilestones",
                table: "ProjectMilestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectMilestones_Order_Positive",
                table: "ProjectMilestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectMilestones_Title_NotEmpty",
                table: "ProjectMilestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectMilestones_WorkflowJobName_NotEmpty",
                table: "ProjectMilestones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CapstoneProjects",
                table: "CapstoneProjects");

            migrationBuilder.DropIndex(
                name: "IX_CapstoneProjects_TaskId",
                table: "CapstoneProjects");

            migrationBuilder.DropIndex(
                name: "IX_CapstoneProjects_TrackId",
                table: "CapstoneProjects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CapstoneProjects_Title_NotEmpty",
                table: "CapstoneProjects");

            migrationBuilder.RenameTable(
                name: "ProjectMilestones",
                newName: "ProjectMilestone");

            migrationBuilder.RenameTable(
                name: "CapstoneProjects",
                newName: "CapstoneProject");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMilestones_ProjectId_WorkflowJobName",
                table: "ProjectMilestone",
                newName: "IX_ProjectMilestone_ProjectId_WorkflowJobName");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMilestones_ProjectId_Order",
                table: "ProjectMilestone",
                newName: "IX_ProjectMilestone_ProjectId_Order");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMilestones_ModuleId",
                table: "ProjectMilestone",
                newName: "IX_ProjectMilestone_ModuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectMilestone",
                table: "ProjectMilestone",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CapstoneProject",
                table: "CapstoneProject",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProject_TaskId",
                table: "CapstoneProject",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CapstoneProject_TrackId",
                table: "CapstoneProject",
                column: "TrackId");

            migrationBuilder.AddForeignKey(
                name: "FK_CapstoneProject_GitHubTask_TaskId",
                table: "CapstoneProject",
                column: "TaskId",
                principalTable: "GitHubTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CapstoneProject_Tracks_TrackId",
                table: "CapstoneProject",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMilestone_CapstoneProject_ProjectId",
                table: "ProjectMilestone",
                column: "ProjectId",
                principalTable: "CapstoneProject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMilestone_Modules_ModuleId",
                table: "ProjectMilestone",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");
        }
    }
}
