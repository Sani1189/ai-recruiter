using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddversionOptional_InterviewConfiguration_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "QuestionsPromptVersion",
                table: "InterviewConfigurations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PersonalityPromptVersion",
                table: "InterviewConfigurations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "InstructionPromptVersion",
                table: "InterviewConfigurations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_InstructionPromptName",
                table: "InterviewConfigurations",
                column: "InstructionPromptName");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_PersonalityPromptName",
                table: "InterviewConfigurations",
                column: "PersonalityPromptName");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_QuestionsPromptName",
                table: "InterviewConfigurations",
                column: "QuestionsPromptName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterviewConfigurations_InstructionPromptName",
                table: "InterviewConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_InterviewConfigurations_PersonalityPromptName",
                table: "InterviewConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_InterviewConfigurations_QuestionsPromptName",
                table: "InterviewConfigurations");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionsPromptVersion",
                table: "InterviewConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonalityPromptVersion",
                table: "InterviewConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InstructionPromptVersion",
                table: "InterviewConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
