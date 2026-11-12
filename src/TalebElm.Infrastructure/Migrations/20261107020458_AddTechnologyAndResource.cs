using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnologyAndResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Technologies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technologies", x => x.Id);
                    table.CheckConstraint("CK_Technologies_Name_NotEmpty", "length(trim(Name)) > 0");
                    table.CheckConstraint("CK_Technologies_Order_Positive", "\"Order\" > 0");
                    table.CheckConstraint("CK_Technologies_Slug_NotEmpty", "length(trim(Slug)) > 0");
                    table.ForeignKey(
                        name: "FK_Technologies_Technologies_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Citation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsecutiveFailureCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCheckedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.CheckConstraint("CK_Resources_ConsecutiveFailureCount_NonNegative", "ConsecutiveFailureCount >= 0");
                    table.CheckConstraint("CK_Resources_Language_NotEmpty", "length(trim(Language)) > 0");
                    table.CheckConstraint("CK_Resources_Title_NotEmpty", "length(trim(Title)) > 0");
                    table.ForeignKey(
                        name: "FK_Resources_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Resources_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CreatedByUserId",
                table: "Resources",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_TechnologyId_Status",
                table: "Resources",
                columns: new[] { "TechnologyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_Order",
                table: "Technologies",
                column: "Order",
                unique: true,
                filter: "\"ParentId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_ParentId_Order",
                table: "Technologies",
                columns: new[] { "ParentId", "Order" },
                unique: true,
                filter: "\"ParentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_ParentId_Slug",
                table: "Technologies",
                columns: new[] { "ParentId", "Slug" },
                unique: true,
                filter: "\"ParentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_Slug",
                table: "Technologies",
                column: "Slug",
                unique: true,
                filter: "\"ParentId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Technologies");
        }
    }
}
