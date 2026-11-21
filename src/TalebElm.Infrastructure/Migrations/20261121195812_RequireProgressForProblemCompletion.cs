using Microsoft.EntityFrameworkCore.Migrations;

namespace TalebElm.Infrastructure.Migrations;

public partial class RequireProgressForProblemCompletion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Reuse the unique learner/Module progress index. A unique index on
        // ModuleProblem supplies the other parent key without rebuilding that table.
        migrationBuilder.Sql("""
            CREATE TEMP TABLE "__CompletionProgressAudit" (
                "Invalid" INTEGER NOT NULL CONSTRAINT "CK_UserProblemCompletions_MatchingProgress" CHECK ("Invalid" = 0)
            );
            INSERT INTO "__CompletionProgressAudit"
            SELECT 1
            WHERE EXISTS (
                SELECT 1 FROM "UserProblemCompletions" AS c
                LEFT JOIN "Users" AS u ON u."Id" = c."UserId"
                LEFT JOIN "ModuleProblem" AS problem ON problem."Id" = c."ModuleProblemId"
                LEFT JOIN "Modules" AS m ON m."Id" = problem."ModuleId"
                LEFT JOIN "UserProgresses" AS p ON p."UserId" = c."UserId" AND p."ModuleId" = problem."ModuleId"
                WHERE u."Id" IS NULL OR m."Id" IS NULL OR p."Id" IS NULL
            );
            DROP TABLE "__CompletionProgressAudit";

            CREATE UNIQUE INDEX "AK_ModuleProblem_Id_ModuleId" ON "ModuleProblem" ("Id", "ModuleId");
            CREATE TABLE "__UserProblemCompletions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_UserProblemCompletions" PRIMARY KEY,
                "CreatedAt" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "ModuleProblemId" TEXT NOT NULL,
                "ModuleId" TEXT NOT NULL,
                "ExternalSubmissionId" TEXT NULL,
                "VerifiedAtUtc" TEXT NOT NULL,
                "DetailsJson" TEXT NULL,
                "VerifiedHeadCommitSha" TEXT NULL,
                "TaskVersionId" TEXT NULL,
                CONSTRAINT "FK_UserProblemCompletions_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_UserProblemCompletions_ModuleProblem_ModuleProblemId_ModuleId"
                    FOREIGN KEY ("ModuleProblemId", "ModuleId") REFERENCES "ModuleProblem" ("Id", "ModuleId") ON DELETE RESTRICT,
                CONSTRAINT "FK_UserProblemCompletions_UserProgresses_UserId_ModuleId"
                    FOREIGN KEY ("UserId", "ModuleId") REFERENCES "UserProgresses" ("UserId", "ModuleId") ON DELETE RESTRICT
            );
            INSERT INTO "__UserProblemCompletions" (
                "Id", "CreatedAt", "UserId", "ModuleProblemId", "ModuleId", "ExternalSubmissionId",
                "VerifiedAtUtc", "DetailsJson", "VerifiedHeadCommitSha", "TaskVersionId"
            )
            SELECT c."Id", c."CreatedAt", c."UserId", c."ModuleProblemId", problem."ModuleId", c."ExternalSubmissionId",
                   c."VerifiedAtUtc", c."DetailsJson", c."VerifiedHeadCommitSha", c."TaskVersionId"
            FROM "UserProblemCompletions" AS c JOIN "ModuleProblem" AS problem ON problem."Id" = c."ModuleProblemId";

            DROP TABLE "UserProblemCompletions";
            ALTER TABLE "__UserProblemCompletions" RENAME TO "UserProblemCompletions";
            CREATE UNIQUE INDEX "IX_UserProblemCompletions_UserId_ModuleProblemId" ON "UserProblemCompletions" ("UserId", "ModuleProblemId");
            CREATE INDEX "IX_UserProblemCompletions_ModuleProblemId_ModuleId" ON "UserProblemCompletions" ("ModuleProblemId", "ModuleId");
            CREATE INDEX "IX_UserProblemCompletions_UserId_ModuleId" ON "UserProblemCompletions" ("UserId", "ModuleId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "__UserProblemCompletions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_UserProblemCompletions" PRIMARY KEY,
                "CreatedAt" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "ModuleProblemId" TEXT NOT NULL,
                "ExternalSubmissionId" TEXT NULL,
                "VerifiedAtUtc" TEXT NOT NULL,
                "DetailsJson" TEXT NULL,
                "VerifiedHeadCommitSha" TEXT NULL,
                "TaskVersionId" TEXT NULL,
                CONSTRAINT "FK_UserProblemCompletions_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_UserProblemCompletions_ModuleProblem_ModuleProblemId"
                    FOREIGN KEY ("ModuleProblemId") REFERENCES "ModuleProblem" ("Id") ON DELETE RESTRICT
            );
            INSERT INTO "__UserProblemCompletions" (
                "Id", "CreatedAt", "UserId", "ModuleProblemId", "ExternalSubmissionId",
                "VerifiedAtUtc", "DetailsJson", "VerifiedHeadCommitSha", "TaskVersionId"
            )
            SELECT "Id", "CreatedAt", "UserId", "ModuleProblemId", "ExternalSubmissionId",
                   "VerifiedAtUtc", "DetailsJson", "VerifiedHeadCommitSha", "TaskVersionId" FROM "UserProblemCompletions";

            DROP TABLE "UserProblemCompletions";
            ALTER TABLE "__UserProblemCompletions" RENAME TO "UserProblemCompletions";
            CREATE UNIQUE INDEX "IX_UserProblemCompletions_UserId_ModuleProblemId" ON "UserProblemCompletions" ("UserId", "ModuleProblemId");
            CREATE INDEX "IX_UserProblemCompletions_ModuleProblemId" ON "UserProblemCompletions" ("ModuleProblemId");
            DROP INDEX "AK_ModuleProblem_Id_ModuleId";
            """);
    }
}
