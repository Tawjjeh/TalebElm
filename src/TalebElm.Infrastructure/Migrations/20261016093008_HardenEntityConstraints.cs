using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalebElm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenEntityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Modules_TrackId",
                table: "Modules");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "!");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Email_NotEmpty",
                table: "Users",
                sql: "length(trim(Email)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Name_NotEmpty",
                table: "Users",
                sql: "length(trim(Name)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_PasswordHash_NotEmpty",
                table: "Users",
                sql: "length(PasswordHash) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_ModuleId",
                table: "UserProgresses",
                column: "ModuleId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserProgress_Score_Range",
                table: "UserProgresses",
                sql: "Score >= 0 AND Score <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tracks_Description_NotEmpty",
                table: "Tracks",
                sql: "length(trim(Description)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tracks_Name_NotEmpty",
                table: "Tracks",
                sql: "length(trim(Name)) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_TrackId_Order",
                table: "Modules",
                columns: new[] { "TrackId", "Order" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Modules_Order_Positive",
                table: "Modules",
                sql: "\"Order\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Modules_Title_NotEmpty",
                table: "Modules",
                sql: "length(trim(Title)) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ModuleId_Order",
                table: "Lessons",
                columns: new[] { "ModuleId", "Order" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lessons_Order_Positive",
                table: "Lessons",
                sql: "\"Order\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lessons_Title_NotEmpty",
                table: "Lessons",
                sql: "length(trim(Title)) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ModuleId",
                table: "Exams",
                column: "ModuleId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Exams_PassThreshold_Range",
                table: "Exams",
                sql: "PassThreshold >= 0 AND PassThreshold <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Exams_Title_NotEmpty",
                table: "Exams",
                sql: "length(trim(Title)) > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Modules_ModuleId",
                table: "Exams",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgresses_Modules_ModuleId",
                table: "UserProgresses",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgresses_Users_UserId",
                table: "UserProgresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Modules_ModuleId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgresses_Modules_ModuleId",
                table: "UserProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgresses_Users_UserId",
                table: "UserProgresses");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Email_NotEmpty",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Name_NotEmpty",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_PasswordHash_NotEmpty",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserProgresses_ModuleId",
                table: "UserProgresses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserProgress_Score_Range",
                table: "UserProgresses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tracks_Description_NotEmpty",
                table: "Tracks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tracks_Name_NotEmpty",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Modules_TrackId_Order",
                table: "Modules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Modules_Order_Positive",
                table: "Modules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Modules_Title_NotEmpty",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_ModuleId_Order",
                table: "Lessons");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lessons_Order_Positive",
                table: "Lessons");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lessons_Title_NotEmpty",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Exams_ModuleId",
                table: "Exams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Exams_PassThreshold_Range",
                table: "Exams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Exams_Title_NotEmpty",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldCollation: "NOCASE");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_TrackId",
                table: "Modules",
                column: "TrackId");
        }
    }
}
