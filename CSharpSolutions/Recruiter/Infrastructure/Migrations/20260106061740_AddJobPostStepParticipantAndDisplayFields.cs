using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPostStepParticipantAndDisplayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayContent",
                table: "JobPostSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayTitle",
                table: "JobPostSteps",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Participant",
                table: "JobPostSteps",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Candidate");

            migrationBuilder.AddColumn<bool>(
                name: "ShowSpinner",
                table: "JobPostSteps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Normalize legacy StepType values that are no longer valid in the new dropdown model
            migrationBuilder.Sql(
                """
                UPDATE JobPostSteps
                SET StepType = 'Generic'
                WHERE StepType IN ('Recruiter Decision', 'Recruiter Step')
                """);

            // Backfill participant from legacy flag (kept for backward compatibility)
            migrationBuilder.Sql(
                """
                UPDATE JobPostSteps
                SET Participant = CASE
                    WHEN RecruiterCompletesStepManually = 1 THEN 'Recruiter'
                    ELSE 'Candidate'
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostSteps_Participant",
                table: "JobPostSteps",
                column: "Participant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPostSteps_Participant",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "DisplayContent",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "DisplayTitle",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "Participant",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "ShowSpinner",
                table: "JobPostSteps");
        }
    }
}
