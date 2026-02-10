using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Candidate_And_Interview_Columns_Update_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterviewQuestions",
                table: "Interviews");

            migrationBuilder.AddColumn<string>(
                name: "CandidateId",
                table: "Candidates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateId",
                table: "Candidates");

            migrationBuilder.AddColumn<string>(
                name: "InterviewQuestions",
                table: "Interviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
