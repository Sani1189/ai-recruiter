using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_CountryExposureSet_Tables_And_Reference_to_All_GdprBaseModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "VolunteerExtracurricular",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "UserProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Summaries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Skills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Scorings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Scores",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "QuestionnaireTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "QuestionnaireSections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "QuestionnaireQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "QuestionnaireQuestionOptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Prompts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "ProjectsResearch",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "KeyStrengths",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "JobPostSteps",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "JobPostStepAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "JobPosts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "JobApplicationSteps",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "JobApplicationStepFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "JobApplications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "JobAdCountryExposure",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Interviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "InterviewConfigurations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Experiences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Educations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "CvEvaluations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "CertificationsLicenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "Candidates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryExposureSetId",
                table: "AwardsAchievements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CountryExposureSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Canonical = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryExposureSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CountryExposureSetCountries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryExposureSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryExposureSetCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryExposureSetCountries_CountryExposureSets_CountryExposureSetId",
                        column: x => x.CountryExposureSetId,
                        principalTable: "CountryExposureSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CountryExposureSetCountries_Country_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "CountryCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerExtracurricular_CountryExposureSetId",
                table: "VolunteerExtracurricular",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_CountryExposureSetId",
                table: "UserProfiles",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Summaries_CountryExposureSetId",
                table: "Summaries",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_CountryExposureSetId",
                table: "Skills",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Scorings_CountryExposureSetId",
                table: "Scorings",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_CountryExposureSetId",
                table: "Scores",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CountryExposureSetId",
                table: "QuestionnaireTemplates",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireSections_CountryExposureSetId",
                table: "QuestionnaireSections",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestions_CountryExposureSetId",
                table: "QuestionnaireQuestions",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestionOptions_CountryExposureSetId",
                table: "QuestionnaireQuestionOptions",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireCandidateSubmissions_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissions",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireCandidateSubmissionAnswers_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswers",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireCandidateSubmissionAnswerOptions_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_CountryExposureSetId",
                table: "Prompts",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectsResearch_CountryExposureSetId",
                table: "ProjectsResearch",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyStrengths_CountryExposureSetId",
                table: "KeyStrengths",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostSteps_CountryExposureSetId",
                table: "JobPostSteps",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_CountryExposureSetId",
                table: "JobPostStepAssignments",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_CountryExposureSetId",
                table: "JobPosts",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationSteps_CountryExposureSetId",
                table: "JobApplicationSteps",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStepFiles_CountryExposureSetId",
                table: "JobApplicationStepFiles",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CountryExposureSetId",
                table: "JobApplications",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobAdCountryExposure_CountryExposureSetId",
                table: "JobAdCountryExposure",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_CountryExposureSetId",
                table: "Interviews",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_CountryExposureSetId",
                table: "InterviewConfigurations",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_CountryExposureSetId",
                table: "Files",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CountryExposureSetId",
                table: "Feedbacks",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_CountryExposureSetId",
                table: "Experiences",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Educations_CountryExposureSetId",
                table: "Educations",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CvEvaluations_CountryExposureSetId",
                table: "CvEvaluations",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CountryExposureSetId",
                table: "Comments",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationsLicenses_CountryExposureSetId",
                table: "CertificationsLicenses",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_CountryExposureSetId",
                table: "Candidates",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AwardsAchievements_CountryExposureSetId",
                table: "AwardsAchievements",
                column: "CountryExposureSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryExposureSetCountries_CountryCode",
                table: "CountryExposureSetCountries",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_CountryExposureSetCountries_CountryExposureSetId_CountryCode",
                table: "CountryExposureSetCountries",
                columns: new[] { "CountryExposureSetId", "CountryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryExposureSets_Canonical",
                table: "CountryExposureSets",
                column: "Canonical",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AwardsAchievements_CountryExposureSets_CountryExposureSetId",
                table: "AwardsAchievements",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_CountryExposureSets_CountryExposureSetId",
                table: "Candidates",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CertificationsLicenses_CountryExposureSets_CountryExposureSetId",
                table: "CertificationsLicenses",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_CountryExposureSets_CountryExposureSetId",
                table: "Comments",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CvEvaluations_CountryExposureSets_CountryExposureSetId",
                table: "CvEvaluations",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Educations_CountryExposureSets_CountryExposureSetId",
                table: "Educations",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Experiences_CountryExposureSets_CountryExposureSetId",
                table: "Experiences",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_CountryExposureSets_CountryExposureSetId",
                table: "Feedbacks",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_CountryExposureSets_CountryExposureSetId",
                table: "Files",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewConfigurations_CountryExposureSets_CountryExposureSetId",
                table: "InterviewConfigurations",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_CountryExposureSets_CountryExposureSetId",
                table: "Interviews",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobAdCountryExposure_CountryExposureSets_CountryExposureSetId",
                table: "JobAdCountryExposure",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_CountryExposureSets_CountryExposureSetId",
                table: "JobApplications",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationStepFiles_CountryExposureSets_CountryExposureSetId",
                table: "JobApplicationStepFiles",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationSteps_CountryExposureSets_CountryExposureSetId",
                table: "JobApplicationSteps",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_CountryExposureSets_CountryExposureSetId",
                table: "JobPosts",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostStepAssignments_CountryExposureSets_CountryExposureSetId",
                table: "JobPostStepAssignments",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSteps_CountryExposureSets_CountryExposureSetId",
                table: "JobPostSteps",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_KeyStrengths_CountryExposureSets_CountryExposureSetId",
                table: "KeyStrengths",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectsResearch_CountryExposureSets_CountryExposureSetId",
                table: "ProjectsResearch",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Prompts_CountryExposureSets_CountryExposureSetId",
                table: "Prompts",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireCandidateSubmissionAnswerOptions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireCandidateSubmissionAnswers_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswers",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireCandidateSubmissions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissions",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireQuestionOptions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireQuestionOptions",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireQuestions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireQuestions",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireSections_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireSections",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireTemplates_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireTemplates",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_CountryExposureSets_CountryExposureSetId",
                table: "Scores",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Scorings_CountryExposureSets_CountryExposureSetId",
                table: "Scorings",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_CountryExposureSets_CountryExposureSetId",
                table: "Skills",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Summaries_CountryExposureSets_CountryExposureSetId",
                table: "Summaries",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_CountryExposureSets_CountryExposureSetId",
                table: "UserProfiles",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerExtracurricular_CountryExposureSets_CountryExposureSetId",
                table: "VolunteerExtracurricular",
                column: "CountryExposureSetId",
                principalTable: "CountryExposureSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AwardsAchievements_CountryExposureSets_CountryExposureSetId",
                table: "AwardsAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_CountryExposureSets_CountryExposureSetId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_CertificationsLicenses_CountryExposureSets_CountryExposureSetId",
                table: "CertificationsLicenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_CountryExposureSets_CountryExposureSetId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_CvEvaluations_CountryExposureSets_CountryExposureSetId",
                table: "CvEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Educations_CountryExposureSets_CountryExposureSetId",
                table: "Educations");

            migrationBuilder.DropForeignKey(
                name: "FK_Experiences_CountryExposureSets_CountryExposureSetId",
                table: "Experiences");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_CountryExposureSets_CountryExposureSetId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_CountryExposureSets_CountryExposureSetId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewConfigurations_CountryExposureSets_CountryExposureSetId",
                table: "InterviewConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_CountryExposureSets_CountryExposureSetId",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_JobAdCountryExposure_CountryExposureSets_CountryExposureSetId",
                table: "JobAdCountryExposure");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_CountryExposureSets_CountryExposureSetId",
                table: "JobApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplicationStepFiles_CountryExposureSets_CountryExposureSetId",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplicationSteps_CountryExposureSets_CountryExposureSetId",
                table: "JobApplicationSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_CountryExposureSets_CountryExposureSetId",
                table: "JobPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPostStepAssignments_CountryExposureSets_CountryExposureSetId",
                table: "JobPostStepAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPostSteps_CountryExposureSets_CountryExposureSetId",
                table: "JobPostSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_KeyStrengths_CountryExposureSets_CountryExposureSetId",
                table: "KeyStrengths");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectsResearch_CountryExposureSets_CountryExposureSetId",
                table: "ProjectsResearch");

            migrationBuilder.DropForeignKey(
                name: "FK_Prompts_CountryExposureSets_CountryExposureSetId",
                table: "Prompts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireCandidateSubmissionAnswerOptions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireCandidateSubmissionAnswers_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireCandidateSubmissions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireQuestionOptions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireQuestions_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireSections_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireSections");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireTemplates_CountryExposureSets_CountryExposureSetId",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_CountryExposureSets_CountryExposureSetId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scorings_CountryExposureSets_CountryExposureSetId",
                table: "Scorings");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_CountryExposureSets_CountryExposureSetId",
                table: "Skills");

            migrationBuilder.DropForeignKey(
                name: "FK_Summaries_CountryExposureSets_CountryExposureSetId",
                table: "Summaries");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_CountryExposureSets_CountryExposureSetId",
                table: "UserProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerExtracurricular_CountryExposureSets_CountryExposureSetId",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropTable(
                name: "CountryExposureSetCountries");

            migrationBuilder.DropTable(
                name: "CountryExposureSets");

            migrationBuilder.DropIndex(
                name: "IX_VolunteerExtracurricular_CountryExposureSetId",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_CountryExposureSetId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Summaries_CountryExposureSetId",
                table: "Summaries");

            migrationBuilder.DropIndex(
                name: "IX_Skills_CountryExposureSetId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Scorings_CountryExposureSetId",
                table: "Scorings");

            migrationBuilder.DropIndex(
                name: "IX_Scores_CountryExposureSetId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_CountryExposureSetId",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireSections_CountryExposureSetId",
                table: "QuestionnaireSections");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireQuestions_CountryExposureSetId",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireQuestionOptions_CountryExposureSetId",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireCandidateSubmissions_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireCandidateSubmissionAnswers_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireCandidateSubmissionAnswerOptions_CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_Prompts_CountryExposureSetId",
                table: "Prompts");

            migrationBuilder.DropIndex(
                name: "IX_ProjectsResearch_CountryExposureSetId",
                table: "ProjectsResearch");

            migrationBuilder.DropIndex(
                name: "IX_KeyStrengths_CountryExposureSetId",
                table: "KeyStrengths");

            migrationBuilder.DropIndex(
                name: "IX_JobPostSteps_CountryExposureSetId",
                table: "JobPostSteps");

            migrationBuilder.DropIndex(
                name: "IX_JobPostStepAssignments_CountryExposureSetId",
                table: "JobPostStepAssignments");

            migrationBuilder.DropIndex(
                name: "IX_JobPosts_CountryExposureSetId",
                table: "JobPosts");

            migrationBuilder.DropIndex(
                name: "IX_JobApplicationSteps_CountryExposureSetId",
                table: "JobApplicationSteps");

            migrationBuilder.DropIndex(
                name: "IX_JobApplicationStepFiles_CountryExposureSetId",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CountryExposureSetId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobAdCountryExposure_CountryExposureSetId",
                table: "JobAdCountryExposure");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_CountryExposureSetId",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_InterviewConfigurations_CountryExposureSetId",
                table: "InterviewConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Files_CountryExposureSetId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_CountryExposureSetId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Experiences_CountryExposureSetId",
                table: "Experiences");

            migrationBuilder.DropIndex(
                name: "IX_Educations_CountryExposureSetId",
                table: "Educations");

            migrationBuilder.DropIndex(
                name: "IX_CvEvaluations_CountryExposureSetId",
                table: "CvEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_Comments_CountryExposureSetId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_CertificationsLicenses_CountryExposureSetId",
                table: "CertificationsLicenses");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_CountryExposureSetId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_AwardsAchievements_CountryExposureSetId",
                table: "AwardsAchievements");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CountryExposureSetId",
                table: "AwardsAchievements");
        }
    }
}
