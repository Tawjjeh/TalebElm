using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TightenExamPassThresholdCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Exams_PassThreshold_Range",
                table: "Exams");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Exams_PassThreshold_Range",
                table: "Exams",
                sql: "PassThreshold >= 1 AND PassThreshold <= 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Exams_PassThreshold_Range",
                table: "Exams");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Exams_PassThreshold_Range",
                table: "Exams",
                sql: "PassThreshold >= 0 AND PassThreshold <= 100");
        }
    }
}
