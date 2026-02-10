using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShowStepForCandidateAndRemoveManualFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowStepForCandidate",
                table: "JobPostSteps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Default: candidate steps are visible, recruiter steps are hidden unless explicitly enabled
            migrationBuilder.Sql(
                """
                UPDATE JobPostSteps
                SET ShowStepForCandidate = CASE
                    WHEN Participant = 'Candidate' THEN 1
                    ELSE 0
                END
                """);

            migrationBuilder.DropColumn(
                name: "RecruiterCompletesStepManually",
                table: "JobPostSteps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecruiterCompletesStepManually",
                table: "JobPostSteps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropColumn(
                name: "ShowStepForCandidate",
                table: "JobPostSteps");
        }
    }
}
