using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Relation_in_File_Model_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Candidates_CandidateId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_JobApplicationStepFiles_FileId",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropIndex(
                name: "IX_Files_CandidateId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CandidateId",
                table: "Files");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStepFiles_FileId",
                table: "JobApplicationStepFiles",
                column: "FileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplicationStepFiles_FileId",
                table: "JobApplicationStepFiles");

            migrationBuilder.AddColumn<Guid>(
                name: "CandidateId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStepFiles_FileId",
                table: "JobApplicationStepFiles",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_CandidateId",
                table: "Files",
                column: "CandidateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Candidates_CandidateId",
                table: "Files",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "Id");
        }
    }
}
