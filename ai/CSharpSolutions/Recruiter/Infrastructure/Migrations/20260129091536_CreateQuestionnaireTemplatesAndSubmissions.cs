using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateQuestionnaireTemplatesAndSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if old tables/constraints exist (from original migration in production)
            // If they don't exist, skip the drop operations (fresh database scenario)
            migrationBuilder.Sql(@"
                -- Drop foreign key if it exists
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JobPostSteps_AssessmentTemplates_AssessmentTemplateName_AssessmentTemplateVersion')
                BEGIN
                    ALTER TABLE JobPostSteps DROP CONSTRAINT FK_JobPostSteps_AssessmentTemplates_AssessmentTemplateName_AssessmentTemplateVersion;
                END

                -- Drop old tables if they exist (order matters: child tables first)
                IF OBJECT_ID('QuestionnaireAnswerOptions', 'U') IS NOT NULL DROP TABLE QuestionnaireAnswerOptions;
                IF OBJECT_ID('AssessmentOptions', 'U') IS NOT NULL DROP TABLE AssessmentOptions;
                IF OBJECT_ID('QuestionnaireAnswers', 'U') IS NOT NULL DROP TABLE QuestionnaireAnswers;
                IF OBJECT_ID('AssessmentQuestions', 'U') IS NOT NULL DROP TABLE AssessmentQuestions;
                IF OBJECT_ID('QuestionnaireSubmissions', 'U') IS NOT NULL DROP TABLE QuestionnaireSubmissions;
                IF OBJECT_ID('AssessmentSections', 'U') IS NOT NULL DROP TABLE AssessmentSections;
                IF OBJECT_ID('AssessmentTemplates', 'U') IS NOT NULL DROP TABLE AssessmentTemplates;
            ");

            // Rename columns if they exist, otherwise add them
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JobPostSteps]') AND name = 'AssessmentTemplateVersion')
                BEGIN
                    EXEC sp_rename N'[dbo].[JobPostSteps].[AssessmentTemplateVersion]', N'QuestionnaireTemplateVersion', 'COLUMN';
                END
                ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JobPostSteps]') AND name = 'QuestionnaireTemplateVersion')
                BEGIN
                    ALTER TABLE [dbo].[JobPostSteps] ADD QuestionnaireTemplateVersion INT NULL;
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JobPostSteps]') AND name = 'AssessmentTemplateName')
                BEGIN
                    EXEC sp_rename N'[dbo].[JobPostSteps].[AssessmentTemplateName]', N'QuestionnaireTemplateName', 'COLUMN';
                END
                ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JobPostSteps]') AND name = 'QuestionnaireTemplateName')
                BEGIN
                    ALTER TABLE [dbo].[JobPostSteps] ADD QuestionnaireTemplateName NVARCHAR(255) NULL;
                END
            ");

            // Rename index if it exists, otherwise create it
            migrationBuilder.Sql(@"
                DECLARE @JobPostStepsObjectId INT = OBJECT_ID(N'[dbo].[JobPostSteps]');

                IF @JobPostStepsObjectId IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = N'IX_JobPostSteps_AssessmentTemplateName_AssessmentTemplateVersion'
                          AND object_id = @JobPostStepsObjectId
                    )
                    BEGIN
                        -- For indexes, sp_rename requires a fully-qualified identifier: schema.table.index
                        EXEC sp_rename
                            N'[dbo].[JobPostSteps].[IX_JobPostSteps_AssessmentTemplateName_AssessmentTemplateVersion]',
                            N'IX_JobPostSteps_QuestionnaireTemplateName_QuestionnaireTemplateVersion',
                            N'INDEX';
                    END
                    ELSE IF NOT EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = N'IX_JobPostSteps_QuestionnaireTemplateName_QuestionnaireTemplateVersion'
                          AND object_id = @JobPostStepsObjectId
                    )
                    BEGIN
                        CREATE INDEX [IX_JobPostSteps_QuestionnaireTemplateName_QuestionnaireTemplateVersion]
                        ON [dbo].[JobPostSteps]([QuestionnaireTemplateName], [QuestionnaireTemplateVersion]);
                    END
                END
            ");

            migrationBuilder.CreateTable(
                name: "QuestionnaireTemplates",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    TemplateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TimeLimitSeconds = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireTemplates", x => new { x.Name, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireCandidateSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    JobApplicationStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireTemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuestionnaireTemplateVersion = table.Column<int>(type: "int", nullable: false),
                    TemplateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSavedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalScore = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxScore = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PersonalityResultJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireCandidateSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireCandidateSubmissions_JobApplicationSteps_JobApplicationStepId",
                        column: x => x.JobApplicationStepId,
                        principalTable: "JobApplicationSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnaireCandidateSubmissions_QuestionnaireTemplates_QuestionnaireTemplateName_QuestionnaireTemplateVersion",
                        columns: x => new { x.QuestionnaireTemplateName, x.QuestionnaireTemplateVersion },
                        principalTable: "QuestionnaireTemplates",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    QuestionnaireTemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuestionnaireTemplateVersion = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireSections_QuestionnaireTemplates_QuestionnaireTemplateName_QuestionnaireTemplateVersion",
                        columns: x => new { x.QuestionnaireTemplateName, x.QuestionnaireTemplateVersion },
                        principalTable: "QuestionnaireTemplates",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireQuestions",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    QuestionnaireSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TraitKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ws = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MediaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MediaUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireQuestions", x => new { x.Name, x.Version });
                    table.ForeignKey(
                        name: "FK_QuestionnaireQuestions_Files_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireQuestions_QuestionnaireSections_QuestionnaireSectionId",
                        column: x => x.QuestionnaireSectionId,
                        principalTable: "QuestionnaireSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireCandidateSubmissionAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    QuestionnaireCandidateSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireQuestionName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuestionnaireQuestionVersion = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoreAwarded = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    WaSum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireCandidateSubmissionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireCandidateSubmissionAnswers_QuestionnaireCandidateSubmissions_QuestionnaireCandidateSubmissionId",
                        column: x => x.QuestionnaireCandidateSubmissionId,
                        principalTable: "QuestionnaireCandidateSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnaireCandidateSubmissionAnswers_QuestionnaireQuestions_QuestionnaireQuestionName_QuestionnaireQuestionVersion",
                        columns: x => new { x.QuestionnaireQuestionName, x.QuestionnaireQuestionVersion },
                        principalTable: "QuestionnaireQuestions",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireQuestionOptions",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    QuestionnaireQuestionName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuestionnaireQuestionVersion = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MediaUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Wa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireQuestionOptions", x => new { x.Name, x.Version });
                    table.ForeignKey(
                        name: "FK_QuestionnaireQuestionOptions_Files_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireQuestionOptions_QuestionnaireQuestions_QuestionnaireQuestionName_QuestionnaireQuestionVersion",
                        columns: x => new { x.QuestionnaireQuestionName, x.QuestionnaireQuestionVersion },
                        principalTable: "QuestionnaireQuestions",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireCandidateSubmissionAnswerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    QuestionnaireCandidateSubmissionAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireQuestionOptionName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuestionnaireQuestionOptionVersion = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Wa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireCandidateSubmissionAnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireCandidateSubmissionAnswerOptions_QuestionnaireCandidateSubmissionAnswers_QuestionnaireCandidateSubmissionAnswer~",
                        column: x => x.QuestionnaireCandidateSubmissionAnswerId,
                        principalTable: "QuestionnaireCandidateSubmissionAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnaireCandidateSubmissionAnswerOptions_QuestionnaireQuestionOptions_QuestionnaireQuestionOptionName_QuestionnaireQues~",
                        columns: x => new { x.QuestionnaireQuestionOptionName, x.QuestionnaireQuestionOptionVersion },
                        principalTable: "QuestionnaireQuestionOptions",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QCSAnsOpt_OptName_OptVer",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                columns: new[] { "QuestionnaireQuestionOptionName", "QuestionnaireQuestionOptionVersion" });

            migrationBuilder.CreateIndex(
                name: "UX_QCSAnsOpt_Ans_OptName_OptVer",
                table: "QuestionnaireCandidateSubmissionAnswerOptions",
                columns: new[] { "QuestionnaireCandidateSubmissionAnswerId", "QuestionnaireQuestionOptionName", "QuestionnaireQuestionOptionVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QCSAns_QQName_QQVer",
                table: "QuestionnaireCandidateSubmissionAnswers",
                columns: new[] { "QuestionnaireQuestionName", "QuestionnaireQuestionVersion" });

            migrationBuilder.CreateIndex(
                name: "UX_QCSAns_Sub_QQName_QQVer",
                table: "QuestionnaireCandidateSubmissionAnswers",
                columns: new[] { "QuestionnaireCandidateSubmissionId", "QuestionnaireQuestionName", "QuestionnaireQuestionVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireCandidateSubmissions_JobApplicationStepId",
                table: "QuestionnaireCandidateSubmissions",
                column: "JobApplicationStepId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireCandidateSubmissions_QuestionnaireTemplateName_QuestionnaireTemplateVersion",
                table: "QuestionnaireCandidateSubmissions",
                columns: new[] { "QuestionnaireTemplateName", "QuestionnaireTemplateVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestionOptions_MediaFileId",
                table: "QuestionnaireQuestionOptions",
                column: "MediaFileId",
                unique: true,
                filter: "[MediaFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_QQOpt_QQName_QQVer_Order",
                table: "QuestionnaireQuestionOptions",
                columns: new[] { "QuestionnaireQuestionName", "QuestionnaireQuestionVersion", "Order" },
                unique: true,
                filter: "[QuestionnaireQuestionVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestions_MediaFileId",
                table: "QuestionnaireQuestions",
                column: "MediaFileId",
                unique: true,
                filter: "[MediaFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireQuestions_QuestionnaireSectionId_Order",
                table: "QuestionnaireQuestions",
                columns: new[] { "QuestionnaireSectionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireSections_QuestionnaireTemplateName_QuestionnaireTemplateVersion_Order",
                table: "QuestionnaireSections",
                columns: new[] { "QuestionnaireTemplateName", "QuestionnaireTemplateVersion", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_Status",
                table: "QuestionnaireTemplates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_TemplateType",
                table: "QuestionnaireTemplates",
                column: "TemplateType");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSteps_QuestionnaireTemplates_QuestionnaireTemplateName_QuestionnaireTemplateVersion",
                table: "JobPostSteps",
                columns: new[] { "QuestionnaireTemplateName", "QuestionnaireTemplateVersion" },
                principalTable: "QuestionnaireTemplates",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostSteps_QuestionnaireTemplates_QuestionnaireTemplateName_QuestionnaireTemplateVersion",
                table: "JobPostSteps");

            migrationBuilder.DropTable(
                name: "QuestionnaireCandidateSubmissionAnswerOptions");

            migrationBuilder.DropTable(
                name: "QuestionnaireCandidateSubmissionAnswers");

            migrationBuilder.DropTable(
                name: "QuestionnaireQuestionOptions");

            migrationBuilder.DropTable(
                name: "QuestionnaireCandidateSubmissions");

            migrationBuilder.DropTable(
                name: "QuestionnaireQuestions");

            migrationBuilder.DropTable(
                name: "QuestionnaireSections");

            migrationBuilder.DropTable(
                name: "QuestionnaireTemplates");

            migrationBuilder.RenameColumn(
                name: "QuestionnaireTemplateVersion",
                table: "JobPostSteps",
                newName: "AssessmentTemplateVersion");

            migrationBuilder.RenameColumn(
                name: "QuestionnaireTemplateName",
                table: "JobPostSteps",
                newName: "AssessmentTemplateName");

            migrationBuilder.RenameIndex(
                name: "IX_JobPostSteps_QuestionnaireTemplateName_QuestionnaireTemplateVersion",
                table: "JobPostSteps",
                newName: "IX_JobPostSteps_AssessmentTemplateName_AssessmentTemplateVersion");

            migrationBuilder.CreateTable(
                name: "AssessmentTemplates",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    TemplateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeLimitSeconds = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentTemplates", x => new { x.Name, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "AssessmentSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AssessmentTemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AssessmentTemplateVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentSections_AssessmentTemplates_AssessmentTemplateName_AssessmentTemplateVersion",
                        columns: x => new { x.AssessmentTemplateName, x.AssessmentTemplateVersion },
                        principalTable: "AssessmentTemplates",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AssessmentTemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JobApplicationStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentTemplateVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastSavedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MaxScore = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PersonalityResultJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TemplateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalScore = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireSubmissions_AssessmentTemplates_AssessmentTemplateName_AssessmentTemplateVersion",
                        columns: x => new { x.AssessmentTemplateName, x.AssessmentTemplateVersion },
                        principalTable: "AssessmentTemplates",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireSubmissions_JobApplicationSteps_JobApplicationStepId",
                        column: x => x.JobApplicationStepId,
                        principalTable: "JobApplicationSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AssessmentSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TraitKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ws = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestions_AssessmentSections_AssessmentSectionId",
                        column: x => x.AssessmentSectionId,
                        principalTable: "AssessmentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestions_Files_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Label = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Wa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentOptions_AssessmentQuestions_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentOptions_Files_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    ScoreAwarded = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WaSum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswers_AssessmentQuestions_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswers_QuestionnaireSubmissions_QuestionnaireSubmissionId",
                        column: x => x.QuestionnaireSubmissionId,
                        principalTable: "QuestionnaireSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireAnswerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AssessmentOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Wa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireAnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswerOptions_AssessmentOptions_AssessmentOptionId",
                        column: x => x.AssessmentOptionId,
                        principalTable: "AssessmentOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswerOptions_QuestionnaireAnswers_QuestionnaireAnswerId",
                        column: x => x.QuestionnaireAnswerId,
                        principalTable: "QuestionnaireAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentOptions_AssessmentQuestionId_Order",
                table: "AssessmentOptions",
                columns: new[] { "AssessmentQuestionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentOptions_MediaFileId",
                table: "AssessmentOptions",
                column: "MediaFileId",
                unique: true,
                filter: "[MediaFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_AssessmentSectionId_Order",
                table: "AssessmentQuestions",
                columns: new[] { "AssessmentSectionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_MediaFileId",
                table: "AssessmentQuestions",
                column: "MediaFileId",
                unique: true,
                filter: "[MediaFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentSections_AssessmentTemplateName_AssessmentTemplateVersion_Order",
                table: "AssessmentSections",
                columns: new[] { "AssessmentTemplateName", "AssessmentTemplateVersion", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentTemplates_Status",
                table: "AssessmentTemplates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentTemplates_TemplateType",
                table: "AssessmentTemplates",
                column: "TemplateType");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswerOptions_AssessmentOptionId",
                table: "QuestionnaireAnswerOptions",
                column: "AssessmentOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswerOptions_QuestionnaireAnswerId_AssessmentOptionId",
                table: "QuestionnaireAnswerOptions",
                columns: new[] { "QuestionnaireAnswerId", "AssessmentOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswers_AssessmentQuestionId",
                table: "QuestionnaireAnswers",
                column: "AssessmentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswers_QuestionnaireSubmissionId_AssessmentQuestionId",
                table: "QuestionnaireAnswers",
                columns: new[] { "QuestionnaireSubmissionId", "AssessmentQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireSubmissions_AssessmentTemplateName_AssessmentTemplateVersion",
                table: "QuestionnaireSubmissions",
                columns: new[] { "AssessmentTemplateName", "AssessmentTemplateVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireSubmissions_JobApplicationStepId",
                table: "QuestionnaireSubmissions",
                column: "JobApplicationStepId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSteps_AssessmentTemplates_AssessmentTemplateName_AssessmentTemplateVersion",
                table: "JobPostSteps",
                columns: new[] { "AssessmentTemplateName", "AssessmentTemplateVersion" },
                principalTable: "AssessmentTemplates",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
