using Microsoft.EntityFrameworkCore.Migrations;

namespace TalebElm.Infrastructure.Migrations;

public partial class RequireMatchingTrackForProjectMilestone : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQLite can use unique indexes for the parent keys, so only the dependent table needs rebuilding.
        migrationBuilder.Sql("""
            CREATE TEMP TABLE "__ProjectMilestoneTrackAudit" (
                "Invalid" INTEGER NOT NULL CONSTRAINT "CK_ProjectMilestones_MatchingTrack" CHECK ("Invalid" = 0)
            );
            INSERT INTO "__ProjectMilestoneTrackAudit"
            SELECT 1
            WHERE EXISTS (
                SELECT 1 FROM "ProjectMilestones" AS s
                LEFT JOIN "CapstoneProjects" AS p ON p."Id" = s."ProjectId"
                LEFT JOIN "Tracks" AS t ON t."Id" = p."TrackId"
                LEFT JOIN "Modules" AS m ON m."Id" = s."ModuleId"
                WHERE t."Id" IS NULL
                   OR (s."ModuleId" IS NOT NULL AND (m."Id" IS NULL OR m."TrackId" <> p."TrackId"))
            );
            DROP TABLE "__ProjectMilestoneTrackAudit";

            CREATE UNIQUE INDEX "AK_Modules_Id_TrackId" ON "Modules" ("Id", "TrackId");
            CREATE UNIQUE INDEX "AK_CapstoneProjects_Id_TrackId" ON "CapstoneProjects" ("Id", "TrackId");
            CREATE TABLE "__ProjectMilestones" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ProjectMilestones" PRIMARY KEY,
                "CreatedAt" TEXT NOT NULL,
                "ProjectId" TEXT NOT NULL,
                "TrackId" TEXT NOT NULL,
                "ModuleId" TEXT NULL,
                "Title" TEXT NOT NULL,
                "Instructions" TEXT NOT NULL,
                "Order" INTEGER NOT NULL,
                "WorkflowJobName" TEXT NOT NULL,
                CONSTRAINT "CK_ProjectMilestones_Title_NotEmpty" CHECK (length(trim(Title)) > 0),
                CONSTRAINT "CK_ProjectMilestones_Order_Positive" CHECK ("Order" > 0),
                CONSTRAINT "CK_ProjectMilestones_WorkflowJobName_NotEmpty" CHECK (length(trim(WorkflowJobName)) > 0),
                CONSTRAINT "FK_ProjectMilestones_CapstoneProjects_ProjectId_TrackId"
                    FOREIGN KEY ("ProjectId", "TrackId") REFERENCES "CapstoneProjects" ("Id", "TrackId") ON DELETE RESTRICT,
                CONSTRAINT "FK_ProjectMilestones_Modules_ModuleId_TrackId"
                    FOREIGN KEY ("ModuleId", "TrackId") REFERENCES "Modules" ("Id", "TrackId") ON DELETE RESTRICT
            );
            INSERT INTO "__ProjectMilestones" (
                "Id", "CreatedAt", "ProjectId", "TrackId", "ModuleId", "Title", "Instructions", "Order", "WorkflowJobName"
            )
            SELECT s."Id", s."CreatedAt", s."ProjectId", p."TrackId", s."ModuleId",
                   s."Title", s."Instructions", s."Order", s."WorkflowJobName"
            FROM "ProjectMilestones" AS s JOIN "CapstoneProjects" AS p ON p."Id" = s."ProjectId";

            DROP TABLE "ProjectMilestones";
            ALTER TABLE "__ProjectMilestones" RENAME TO "ProjectMilestones";
            CREATE UNIQUE INDEX "IX_ProjectMilestones_ProjectId_Order" ON "ProjectMilestones" ("ProjectId", "Order");
            CREATE UNIQUE INDEX "IX_ProjectMilestones_ProjectId_WorkflowJobName" ON "ProjectMilestones" ("ProjectId", "WorkflowJobName");
            CREATE INDEX "IX_ProjectMilestones_ProjectId_TrackId" ON "ProjectMilestones" ("ProjectId", "TrackId");
            CREATE INDEX "IX_ProjectMilestones_ModuleId_TrackId" ON "ProjectMilestones" ("ModuleId", "TrackId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "__ProjectMilestones" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ProjectMilestones" PRIMARY KEY,
                "CreatedAt" TEXT NOT NULL,
                "ProjectId" TEXT NOT NULL,
                "ModuleId" TEXT NULL,
                "Title" TEXT NOT NULL,
                "Instructions" TEXT NOT NULL,
                "Order" INTEGER NOT NULL,
                "WorkflowJobName" TEXT NOT NULL,
                CONSTRAINT "CK_ProjectMilestones_Title_NotEmpty" CHECK (length(trim(Title)) > 0),
                CONSTRAINT "CK_ProjectMilestones_Order_Positive" CHECK ("Order" > 0),
                CONSTRAINT "CK_ProjectMilestones_WorkflowJobName_NotEmpty" CHECK (length(trim(WorkflowJobName)) > 0),
                CONSTRAINT "FK_ProjectMilestones_CapstoneProjects_ProjectId"
                    FOREIGN KEY ("ProjectId") REFERENCES "CapstoneProjects" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_ProjectMilestones_Modules_ModuleId"
                    FOREIGN KEY ("ModuleId") REFERENCES "Modules" ("Id") ON DELETE RESTRICT
            );
            INSERT INTO "__ProjectMilestones" (
                "Id", "CreatedAt", "ProjectId", "ModuleId", "Title", "Instructions", "Order", "WorkflowJobName"
            )
            SELECT "Id", "CreatedAt", "ProjectId", "ModuleId", "Title", "Instructions", "Order", "WorkflowJobName"
            FROM "ProjectMilestones";

            DROP TABLE "ProjectMilestones";
            ALTER TABLE "__ProjectMilestones" RENAME TO "ProjectMilestones";
            CREATE UNIQUE INDEX "IX_ProjectMilestones_ProjectId_Order" ON "ProjectMilestones" ("ProjectId", "Order");
            CREATE UNIQUE INDEX "IX_ProjectMilestones_ProjectId_WorkflowJobName" ON "ProjectMilestones" ("ProjectId", "WorkflowJobName");
            CREATE INDEX "IX_ProjectMilestones_ModuleId" ON "ProjectMilestones" ("ModuleId");
            DROP INDEX "AK_CapstoneProjects_Id_TrackId";
            DROP INDEX "AK_Modules_Id_TrackId";
            """);
    }
}
