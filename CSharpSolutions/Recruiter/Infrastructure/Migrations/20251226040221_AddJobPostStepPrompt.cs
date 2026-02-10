using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPostStepPrompt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromptName",
                table: "JobPostSteps",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromptVersion",
                table: "JobPostSteps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostSteps_PromptName_PromptVersion",
                table: "JobPostSteps",
                columns: new[] { "PromptName", "PromptVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSteps_Prompts_PromptName_PromptVersion",
                table: "JobPostSteps",
                columns: new[] { "PromptName", "PromptVersion" },
                principalTable: "Prompts",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostSteps_Prompts_PromptName_PromptVersion",
                table: "JobPostSteps");

            migrationBuilder.DropIndex(
                name: "IX_JobPostSteps_PromptName_PromptVersion",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "PromptName",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "PromptVersion",
                table: "JobPostSteps");
        }
    }
}
