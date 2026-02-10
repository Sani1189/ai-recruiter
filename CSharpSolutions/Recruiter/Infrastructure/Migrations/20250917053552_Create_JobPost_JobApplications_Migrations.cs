using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_JobPost_JobApplications_Migrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Container = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MbSize = table.Column<int>(type: "int", nullable: false),
                    StorageAccountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobPosts",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    MaxAmountOfCandidatesRestriction = table.Column<int>(type: "int", nullable: false),
                    MinimumRequirements = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExperienceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PoliceReportRequired = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPosts", x => new { x.Name, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => new { x.Name, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ResumeUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTypePreferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenToRelocation = table.Column<bool>(type: "bit", nullable: true),
                    RemotePreferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewConfigurations",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Modality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProbingDepth = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FocusArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InstructionPromptName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    InstructionPromptVersion = table.Column<int>(type: "int", nullable: false),
                    PersonalityPromptName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PersonalityPromptVersion = table.Column<int>(type: "int", nullable: false),
                    QuestionsPromptName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuestionsPromptVersion = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewConfigurations", x => new { x.Name, x.Version });
                    table.ForeignKey(
                        name: "FK_InterviewConfigurations_Prompts_InstructionPromptName_InstructionPromptVersion",
                        columns: x => new { x.InstructionPromptName, x.InstructionPromptVersion },
                        principalTable: "Prompts",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewConfigurations_Prompts_PersonalityPromptName_PersonalityPromptVersion",
                        columns: x => new { x.PersonalityPromptName, x.PersonalityPromptVersion },
                        principalTable: "Prompts",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewConfigurations_Prompts_QuestionsPromptName_QuestionsPromptVersion",
                        columns: x => new { x.QuestionsPromptName, x.QuestionsPromptVersion },
                        principalTable: "Prompts",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CvFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidates_Files_CvFileId",
                        column: x => x.CvFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Candidates_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobPostSteps",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsInterview = table.Column<bool>(type: "bit", nullable: false),
                    StepType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecruiterCompletesStepManually = table.Column<bool>(type: "bit", nullable: false),
                    InterviewConfigurationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    InterviewConfigurationVersion = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostSteps", x => new { x.Name, x.Version });
                    table.ForeignKey(
                        name: "FK_JobPostSteps_InterviewConfigurations_InterviewConfigurationName_InterviewConfigurationVersion",
                        columns: x => new { x.InterviewConfigurationName, x.InterviewConfigurationVersion },
                        principalTable: "InterviewConfigurations",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    JobPostName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JobPostVersion = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 255, nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplications_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_JobPosts_JobPostName_JobPostVersion",
                        columns: x => new { x.JobPostName, x.JobPostVersion },
                        principalTable: "JobPosts",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobPostStepAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    JobPostName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JobPostVersion = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StepVersion = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostStepAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostStepAssignments_JobPostSteps_StepName_StepVersion",
                        columns: x => new { x.StepName, x.StepVersion },
                        principalTable: "JobPostSteps",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPostStepAssignments_JobPosts_JobPostName_JobPostVersion",
                        columns: x => new { x.JobPostName, x.JobPostVersion },
                        principalTable: "JobPosts",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    JobApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPostStepName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JobPostStepVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationSteps_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobApplicationSteps_JobPostSteps_JobPostStepName_JobPostStepVersion",
                        columns: x => new { x.JobPostStepName, x.JobPostStepVersion },
                        principalTable: "JobPostSteps",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    JobApplicationStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InterviewAudioUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InterviewConfigurationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    InterviewConfigurationVersion = table.Column<int>(type: "int", nullable: false),
                    TranscriptUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InterviewQuestions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interviews_InterviewConfigurations_InterviewConfigurationName_InterviewConfigurationVersion",
                        columns: x => new { x.InterviewConfigurationName, x.InterviewConfigurationVersion },
                        principalTable: "InterviewConfigurations",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interviews_JobApplicationSteps_JobApplicationStepId",
                        column: x => x.JobApplicationStepId,
                        principalTable: "JobApplicationSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationStepFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobApplicationStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationStepFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationStepFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobApplicationStepFiles_JobApplicationSteps_JobApplicationStepId",
                        column: x => x.JobApplicationStepId,
                        principalTable: "JobApplicationSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Detailed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Strengths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weaknesses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Average = table.Column<double>(type: "float", nullable: false),
                    English = table.Column<double>(type: "float", nullable: false),
                    Technical = table.Column<double>(type: "float", nullable: false),
                    Communication = table.Column<double>(type: "float", nullable: false),
                    ProblemSolving = table.Column<double>(type: "float", nullable: false),
                    InterviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scores_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_CvFileId",
                table: "Candidates",
                column: "CvFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_UserId",
                table: "Candidates",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InterviewId",
                table: "Feedbacks",
                column: "InterviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_Container",
                table: "Files",
                column: "Container");

            migrationBuilder.CreateIndex(
                name: "IX_Files_Extension",
                table: "Files",
                column: "Extension");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_Active",
                table: "InterviewConfigurations",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_InstructionPromptName_InstructionPromptVersion",
                table: "InterviewConfigurations",
                columns: new[] { "InstructionPromptName", "InstructionPromptVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_Modality",
                table: "InterviewConfigurations",
                column: "Modality");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_PersonalityPromptName_PersonalityPromptVersion",
                table: "InterviewConfigurations",
                columns: new[] { "PersonalityPromptName", "PersonalityPromptVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewConfigurations_QuestionsPromptName_QuestionsPromptVersion",
                table: "InterviewConfigurations",
                columns: new[] { "QuestionsPromptName", "QuestionsPromptVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InterviewConfigurationName_InterviewConfigurationVersion",
                table: "Interviews",
                columns: new[] { "InterviewConfigurationName", "InterviewConfigurationVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_JobApplicationStepId",
                table: "Interviews",
                column: "JobApplicationStepId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CandidateId",
                table: "JobApplications",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobPostName_JobPostVersion",
                table: "JobApplications",
                columns: new[] { "JobPostName", "JobPostVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStepFiles_FileId",
                table: "JobApplicationStepFiles",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStepFiles_FileId_JobApplicationStepId",
                table: "JobApplicationStepFiles",
                columns: new[] { "FileId", "JobApplicationStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStepFiles_JobApplicationStepId",
                table: "JobApplicationStepFiles",
                column: "JobApplicationStepId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationSteps_JobApplicationId",
                table: "JobApplicationSteps",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationSteps_JobPostStepName_JobPostStepVersion",
                table: "JobApplicationSteps",
                columns: new[] { "JobPostStepName", "JobPostStepVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationSteps_Status",
                table: "JobApplicationSteps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_ExperienceLevel",
                table: "JobPosts",
                column: "ExperienceLevel");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_JobType",
                table: "JobPosts",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_Name_Version",
                table: "JobPosts",
                columns: new[] { "Name", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_JobPostName_JobPostVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "JobPostName", "JobPostVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_JobPostName_JobPostVersion_StepName_StepVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "JobPostName", "JobPostVersion", "StepName", "StepVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_JobPostName_JobPostVersion_StepNumber",
                table: "JobPostStepAssignments",
                columns: new[] { "JobPostName", "JobPostVersion", "StepNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_Status",
                table: "JobPostStepAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostStepAssignments_StepName_StepVersion",
                table: "JobPostStepAssignments",
                columns: new[] { "StepName", "StepVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostSteps_InterviewConfigurationName_InterviewConfigurationVersion",
                table: "JobPostSteps",
                columns: new[] { "InterviewConfigurationName", "InterviewConfigurationVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostSteps_IsInterview",
                table: "JobPostSteps",
                column: "IsInterview");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostSteps_StepType",
                table: "JobPostSteps",
                column: "StepType");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_Category",
                table: "Prompts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_Locale",
                table: "Prompts",
                column: "Locale");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_InterviewId",
                table: "Scores",
                column: "InterviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Email",
                table: "UserProfiles",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Name",
                table: "UserProfiles",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "JobApplicationStepFiles");

            migrationBuilder.DropTable(
                name: "JobPostStepAssignments");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "Interviews");

            migrationBuilder.DropTable(
                name: "JobApplicationSteps");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobPostSteps");

            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "JobPosts");

            migrationBuilder.DropTable(
                name: "InterviewConfigurations");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Prompts");
        }
    }
}
