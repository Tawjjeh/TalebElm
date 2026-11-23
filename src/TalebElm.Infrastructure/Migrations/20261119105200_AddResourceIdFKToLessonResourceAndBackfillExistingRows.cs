using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TalebElm.Domain.Enums;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceIdFKToLessonResourceAndBackfillExistingRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonResources_LessonId",
                table: "LessonResources");

            migrationBuilder.AddColumn<Guid>(
                name: "ResourceId",
                table: "LessonResources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonResources_LessonId_Order",
                table: "LessonResources",
                columns: new[] { "LessonId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonResources_ResourceId",
                table: "LessonResources",
                column: "ResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResources_Resources_ResourceId",
                table: "LessonResources",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Map by enum NAME (never an integer cast). Add one WHEN per LessonResourceKind member.
            string Kind(string col) => $@"CASE {col}
                WHEN {(int)LessonResourceKind.Book}          THEN {(int)ResourceKind.Book}
                WHEN {(int)LessonResourceKind.Documentation} THEN {(int)ResourceKind.Documentation}
                WHEN {(int)LessonResourceKind.Video}         THEN {(int)ResourceKind.Video}
                WHEN {(int)LessonResourceKind.Article}       THEN {(int)ResourceKind.Article}
                WHEN {(int)LessonResourceKind.Repository}    THEN {(int)ResourceKind.Repository}
                WHEN {(int)LessonResourceKind.Other}         THEN {(int)ResourceKind.Other}
            END";

            // 1. One Resource per distinct legacy LessonResource
            migrationBuilder.Sql($@"
                INSERT INTO ""Resources"" (""Id"", ""CreatedAt"", ""Title"", ""Kind"", ""Url"", ""Citation"", ""Language"", ""Level"", ""IsPaid"", ""Status"", ""ConsecutiveFailureCount"")
                SELECT MIN(""Id""), CURRENT_TIMESTAMP, ""Title"", {Kind(@"""Kind""")}, ""Url"", ""Citation"", 'en', {(int)default(ResourceLevel)}, FALSE, {(int)ResourceStatus.Approved}, 0
                FROM ""LessonResources""
                GROUP BY ""Title"", {Kind(@"""Kind""")}, ""Url"", ""Citation"";");

            // 2. Point each LessonResource at its Resource
            migrationBuilder.Sql($@"
            UPDATE ""LessonResources"" SET ""ResourceId"" = (
                SELECT r.""Id"" FROM ""Resources"" r
                WHERE r.""Title"" = ""LessonResources"".""Title""
                  AND r.""Kind"" = {Kind(@"""LessonResources"".""Kind""")}
                  AND COALESCE(r.""Url"", '') = COALESCE(""LessonResources"".""Url"", '')
                  AND COALESCE(r.""Citation"", '') = COALESCE(""LessonResources"".""Citation"", '')
                LIMIT 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonResources_Resources_ResourceId",
                table: "LessonResources");

            migrationBuilder.DropIndex(
                name: "IX_LessonResources_LessonId_Order",
                table: "LessonResources");

            migrationBuilder.DropIndex(
                name: "IX_LessonResources_ResourceId",
                table: "LessonResources");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "LessonResources");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResources_LessonId",
                table: "LessonResources",
                column: "LessonId");
        }
    }
}
