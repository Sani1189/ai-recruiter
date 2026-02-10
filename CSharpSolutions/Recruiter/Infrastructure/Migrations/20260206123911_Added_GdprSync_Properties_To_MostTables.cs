using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_GdprSync_Properties_To_MostTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "VolunteerExtracurricular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "VolunteerExtracurricular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "VolunteerExtracurricular",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "VolunteerExtracurricular",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "VolunteerExtracurricular",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "VolunteerExtracurricular",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "VolunteerExtracurricular",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "UserProfiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "UserProfiles",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "UserProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "UserProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "UserProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Summaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Summaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Summaries",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Summaries",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Summaries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Summaries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Summaries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Skills",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Skills",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Skills",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Skills",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Skills",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Scorings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Scorings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Scorings",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Scorings",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Scorings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Scorings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Scorings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Scores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Scores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Scores",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Scores",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Scores",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Scores",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Scores",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "QuestionnaireTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "QuestionnaireTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "QuestionnaireTemplates",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "QuestionnaireTemplates",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "QuestionnaireTemplates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireTemplates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "QuestionnaireTemplates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "QuestionnaireSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "QuestionnaireSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "QuestionnaireSections",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "QuestionnaireSections",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "QuestionnaireSections",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireSections",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "QuestionnaireSections",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "QuestionnaireQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "QuestionnaireQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "QuestionnaireQuestions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "QuestionnaireQuestions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "QuestionnaireQuestions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireQuestions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "QuestionnaireQuestions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "QuestionnaireQuestionOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "QuestionnaireQuestionOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "QuestionnaireQuestionOptions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "QuestionnaireQuestionOptions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "QuestionnaireQuestionOptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireQuestionOptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "QuestionnaireQuestionOptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "QuestionnaireCandidateSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "QuestionnaireCandidateSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "QuestionnaireCandidateSubmissions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "QuestionnaireCandidateSubmissions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "QuestionnaireCandidateSubmissions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireCandidateSubmissions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "QuestionnaireCandidateSubmissions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "QuestionnaireCandidateSubmissionAnswers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Prompts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Prompts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Prompts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Prompts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Prompts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Prompts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Prompts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "ProjectsResearch",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "ProjectsResearch",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "ProjectsResearch",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "ProjectsResearch",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "ProjectsResearch",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "ProjectsResearch",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "ProjectsResearch",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "KeyStrengths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "KeyStrengths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "KeyStrengths",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "KeyStrengths",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "KeyStrengths",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "KeyStrengths",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "KeyStrengths",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "JobPostSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "JobPostSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "JobPostSteps",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "JobPostSteps",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "JobPostSteps",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "JobPostSteps",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "JobPostSteps",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "JobPostStepAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "JobPostStepAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "JobPostStepAssignments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "JobPostStepAssignments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "JobPostStepAssignments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "JobPostStepAssignments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "JobPostStepAssignments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "JobPosts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "JobPosts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "JobPosts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "JobPosts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "JobPosts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "JobApplicationSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "JobApplicationSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "JobApplicationSteps",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "JobApplicationSteps",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "JobApplicationSteps",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "JobApplicationSteps",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "JobApplicationSteps",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "JobApplicationStepFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "JobApplicationStepFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "JobApplicationStepFiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "JobApplicationStepFiles",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "JobApplicationStepFiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "JobApplicationStepFiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "JobApplicationStepFiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "JobApplications",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "JobApplications",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "JobApplications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "JobApplications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "JobApplications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "JobAdCountryExposure",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "JobAdCountryExposure",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "JobAdCountryExposure",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "JobAdCountryExposure",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "JobAdCountryExposure",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "JobAdCountryExposure",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "JobAdCountryExposure",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Interviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Interviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Interviews",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Interviews",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Interviews",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Interviews",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Interviews",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "InterviewConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "InterviewConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "InterviewConfigurations",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "InterviewConfigurations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "InterviewConfigurations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "InterviewConfigurations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "InterviewConfigurations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Files",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Files",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Files",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Files",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Files",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Feedbacks",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Feedbacks",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Feedbacks",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Feedbacks",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Feedbacks",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Experiences",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Experiences",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Experiences",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Experiences",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Experiences",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Educations",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Educations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Educations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Educations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Educations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "CvEvaluations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "CvEvaluations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "CvEvaluations",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "CvEvaluations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "CvEvaluations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "CvEvaluations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "CvEvaluations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Comments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Comments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Comments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Comments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Comments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "CertificationsLicenses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "CertificationsLicenses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "CertificationsLicenses",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "CertificationsLicenses",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "CertificationsLicenses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "CertificationsLicenses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "CertificationsLicenses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "Candidates",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "Candidates",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "Candidates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "Candidates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "Candidates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOriginRegion",
                table: "AwardsAchievements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataResidency",
                table: "AwardsAchievements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSanitized",
                table: "AwardsAchievements",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncEventId",
                table: "AwardsAchievements",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "AwardsAchievements",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizationOverrideConsentAt",
                table: "AwardsAchievements",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SanitizedAt",
                table: "AwardsAchievements",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EntitySyncConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    EntityTypeName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DataClassification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SyncScope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalBasis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalBasisRef = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProcessingPurpose = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RequiresSanitizationForGlobalSync = table.Column<bool>(type: "bit", nullable: false),
                    AllowSanitizationOverrideConsent = table.Column<bool>(type: "bit", nullable: false),
                    DependsOnEntities = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntitySyncConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntitySyncConfigurations_EntityTypeName",
                table: "EntitySyncConfigurations",
                column: "EntityTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntitySyncConfigurations_IsEnabled",
                table: "EntitySyncConfigurations",
                column: "IsEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntitySyncConfigurations");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "VolunteerExtracurricular");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "QuestionnaireSections");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "QuestionnaireQuestions");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "QuestionnaireQuestionOptions");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "ProjectsResearch");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "KeyStrengths");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "JobPostSteps");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "JobPostStepAssignments");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "JobApplicationSteps");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "JobApplicationStepFiles");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "JobAdCountryExposure");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "InterviewConfigurations");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "CvEvaluations");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "CertificationsLicenses");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "DataOriginRegion",
                table: "AwardsAchievements");

            migrationBuilder.DropColumn(
                name: "DataResidency",
                table: "AwardsAchievements");

            migrationBuilder.DropColumn(
                name: "IsSanitized",
                table: "AwardsAchievements");

            migrationBuilder.DropColumn(
                name: "LastSyncEventId",
                table: "AwardsAchievements");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "AwardsAchievements");

            migrationBuilder.DropColumn(
                name: "SanitizationOverrideConsentAt",
                table: "AwardsAchievements");

            migrationBuilder.DropColumn(
                name: "SanitizedAt",
                table: "AwardsAchievements");
        }
    }
}
