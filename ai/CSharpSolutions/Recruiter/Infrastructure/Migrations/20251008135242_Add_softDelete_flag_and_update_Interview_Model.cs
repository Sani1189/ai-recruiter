using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_softDelete_flag_and_update_Interview_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Scores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Prompts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JobPostSteps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JobPostStepAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JobPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JobApplicationSteps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JobApplicationStepFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JobApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "InstructionPromptName",
                table: "Interviews",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InstructionPromptVersion",
                table: "Interviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Interviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PersonalityPromptName",
                table: "Interviews",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PersonalityPromptVersion",
                table: "Interviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuestionsPromptName",
                table: "Interviews",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "QuestionsPromptVersion",
                table: "Interviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InterviewConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Files",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Feedbacks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Candidates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InstructionPromptName",
                table: "Interviews",
                column: "InstructionPromptName");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InstructionPromptName_InstructionPromptVersion",
                table: "Interviews",
                columns: new[] { "InstructionPromptName", "InstructionPromptVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_PersonalityPromptName",
                table: "Interviews",
                column: "PersonalityPromptName");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_PersonalityPromptName_PersonalityPromptVersion",
                table: "Interviews",
                columns: new[] { "PersonalityPromptName", "PersonalityPromptVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_QuestionsPromptName",
                table: "Interviews",
                column: "QuestionsPromptName");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_QuestionsPromptName_QuestionsPromptVersion",
                table: "Interviews",
                columns: new[] { "QuestionsPromptName", "QuestionsPromptVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Prompts_InstructionPromptName_InstructionPromptVersion",
                table: "Interviews",
                columns: new[] { "InstructionPromptName", "InstructionPromptVersion" },
                principalTable: "Prompts",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Prompts_PersonalityPromptName_PersonalityPromptVersion",
                table: "Interviews",
                columns: new[] { "PersonalityPromptName", "PersonalityPromptVersion" },
                principalTable: "Prompts",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Prompts_QuestionsPromptName_QuestionsPromptVersion",
                table: "Interviews",
                columns: new[] { "QuestionsPromptName", "QuestionsPromptVersion" },
                principalTable: "Prompts",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Prompts_InstructionPromptName_InstructionPromptVersion",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Prompts_PersonalityPromptName_PersonalityPromptVersion",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Prompts_QuestionsPromptName_QuestionsPromptVersion",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_InstructionPromptName",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_InstructionPromptName_InstructionPromptVersion",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_PersonalityPromptName",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_PersonalityPromptName_PersonalityPromptVersion",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_QuestionsPromptName",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_QuestionsPromptName_QuestionsPromptVersion",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "InstructionPromptName",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "InstructionPromptVersion",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "PersonalityPromptName",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "PersonalityPromptVersion",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "QuestionsPromptName",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "QuestionsPromptVersion",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Candidates");
        }
    }
}
