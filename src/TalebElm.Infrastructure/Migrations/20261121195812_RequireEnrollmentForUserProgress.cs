using Microsoft.EntityFrameworkCore.Migrations;

namespace TalebElm.Infrastructure.Migrations;

public partial class RequireEnrollmentForUserProgress : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQLite accepts a unique index as a parent key. Reuse the enrollment index
        // and add the Module index without rebuilding either principal table.
        migrationBuilder.Sql("""
            CREATE TEMP TABLE "__ProgressEnrollmentAudit" (
                "Invalid" INTEGER NOT NULL CONSTRAINT "CK_UserProgresses_MatchingEnrollment" CHECK ("Invalid" = 0)
            );
            INSERT INTO "__ProgressEnrollmentAudit"
            SELECT 1
            WHERE EXISTS (
                SELECT 1 FROM "UserProgresses" AS p
                LEFT JOIN "Users" AS u ON u."Id" = p."UserId"
                LEFT JOIN "Modules" AS m ON m."Id" = p."ModuleId"
                LEFT JOIN "Tracks" AS t ON t."Id" = m."TrackId"
                LEFT JOIN "TrackEnrollments" AS e ON e."UserId" = p."UserId" AND e."TrackId" = m."TrackId"
                WHERE u."Id" IS NULL OR t."Id" IS NULL OR e."Id" IS NULL
            );
            DROP TABLE "__ProgressEnrollmentAudit";

            CREATE UNIQUE INDEX "AK_Modules_Id_TrackId" ON "Modules" ("Id", "TrackId");
            CREATE TABLE "__UserProgresses" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_UserProgresses" PRIMARY KEY,
                "CreatedAt" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "ModuleId" TEXT NOT NULL,
                "TrackId" TEXT NOT NULL,
                "IsUnlocked" INTEGER NOT NULL,
                "PassedExam" INTEGER NOT NULL,
                "CompletedRequiredProblems" INTEGER NOT NULL DEFAULT 0,
                "Score" INTEGER NOT NULL,
                CONSTRAINT "CK_UserProgress_Score_Range" CHECK (Score >= 0 AND Score <= 100),
                CONSTRAINT "FK_UserProgresses_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_UserProgresses_Modules_ModuleId_TrackId"
                    FOREIGN KEY ("ModuleId", "TrackId") REFERENCES "Modules" ("Id", "TrackId") ON DELETE RESTRICT,
                CONSTRAINT "FK_UserProgresses_TrackEnrollments_UserId_TrackId"
                    FOREIGN KEY ("UserId", "TrackId") REFERENCES "TrackEnrollments" ("UserId", "TrackId") ON DELETE RESTRICT
            );
            INSERT INTO "__UserProgresses" (
                "Id", "CreatedAt", "UserId", "ModuleId", "TrackId", "IsUnlocked", "PassedExam", "CompletedRequiredProblems", "Score"
            )
            SELECT p."Id", p."CreatedAt", p."UserId", p."ModuleId", m."TrackId", p."IsUnlocked",
                   p."PassedExam", p."CompletedRequiredProblems", p."Score"
            FROM "UserProgresses" AS p JOIN "Modules" AS m ON m."Id" = p."ModuleId";

            DROP TABLE "UserProgresses";
            ALTER TABLE "__UserProgresses" RENAME TO "UserProgresses";
            CREATE UNIQUE INDEX "IX_UserProgresses_UserId_ModuleId" ON "UserProgresses" ("UserId", "ModuleId");
            CREATE INDEX "IX_UserProgresses_ModuleId_TrackId" ON "UserProgresses" ("ModuleId", "TrackId");
            CREATE INDEX "IX_UserProgresses_UserId_TrackId" ON "UserProgresses" ("UserId", "TrackId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "__UserProgresses" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_UserProgresses" PRIMARY KEY,
                "CreatedAt" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "ModuleId" TEXT NOT NULL,
                "IsUnlocked" INTEGER NOT NULL,
                "PassedExam" INTEGER NOT NULL,
                "CompletedRequiredProblems" INTEGER NOT NULL DEFAULT 0,
                "Score" INTEGER NOT NULL,
                CONSTRAINT "CK_UserProgress_Score_Range" CHECK (Score >= 0 AND Score <= 100),
                CONSTRAINT "FK_UserProgresses_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_UserProgresses_Modules_ModuleId"
                    FOREIGN KEY ("ModuleId") REFERENCES "Modules" ("Id") ON DELETE RESTRICT
            );
            INSERT INTO "__UserProgresses" (
                "Id", "CreatedAt", "UserId", "ModuleId", "IsUnlocked", "PassedExam", "CompletedRequiredProblems", "Score"
            )
            SELECT "Id", "CreatedAt", "UserId", "ModuleId", "IsUnlocked", "PassedExam", "CompletedRequiredProblems", "Score"
            FROM "UserProgresses";

            DROP TABLE "UserProgresses";
            ALTER TABLE "__UserProgresses" RENAME TO "UserProgresses";
            CREATE UNIQUE INDEX "IX_UserProgresses_UserId_ModuleId" ON "UserProgresses" ("UserId", "ModuleId");
            CREATE INDEX "IX_UserProgresses_ModuleId" ON "UserProgresses" ("ModuleId");
            DROP INDEX "AK_Modules_Id_TrackId";
            """);
    }
}
