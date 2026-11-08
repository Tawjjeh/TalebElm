using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedRequiredProblemsToUserProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CompletedRequiredProblems",
                table: "UserProgresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedRequiredProblems",
                table: "UserProgresses");
        }
    }
}
