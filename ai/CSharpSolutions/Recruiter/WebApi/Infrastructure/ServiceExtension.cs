using FluentValidation;
using FluentValidation.AspNetCore;
using Recruiter.Application.AwardAchievement;
using Recruiter.Application.AwardAchievement.Interfaces;
using Recruiter.Application.Candidate;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Application.CertificationLicense;
using Recruiter.Application.CertificationLicense.Interfaces;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.CvEvaluation;
using Recruiter.Application.CvEvaluation.Interfaces;
using Recruiter.Application.Education;
using Recruiter.Application.Education.Interfaces;
using Recruiter.Application.Experience;
using Recruiter.Application.Experience.Interfaces;
using Recruiter.Application.File;
using Recruiter.Application.File.Interfaces;
using Recruiter.Application.InterviewConfiguration;
using Recruiter.Application.InterviewConfiguration.Interfaces;
using Recruiter.Application.InterviewConfiguration.Queries;
using Recruiter.Application.JobPost;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobPost.Queries;
using Recruiter.Application.JobPost.Services;
using Recruiter.Application.KeyStrength;
using Recruiter.Application.KeyStrength.Interfaces;
using Recruiter.Application.ProjectResearch;
using Recruiter.Application.ProjectResearch.Interfaces;
using Recruiter.Application.Prompt;
using Recruiter.Application.Prompt.Interfaces;
using Recruiter.Application.Prompt.Queries;
using Recruiter.Application.Scoring;
using Recruiter.Application.Scoring.Interfaces;
using Recruiter.Application.Skill;
using Recruiter.Application.Skill.Interfaces;
using Recruiter.Application.Summary;
using Recruiter.Application.Summary.Interfaces;
using Recruiter.Application.UserProfile;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Application.UserProfile.Queries;
using Recruiter.Application.File.Queries;
using Recruiter.Application.Candidate.Queries;
using Recruiter.Application.VolunteerExtracurricular;
using Recruiter.Application.VolunteerExtracurricular.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Ardalis;
using Recruiter.Infrastructure.Extensions;
using Recruiter.Infrastructure.Repository;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.JobApplicationStepFiles.Interfaces;
using Recruiter.Application.JobApplication;
using Recruiter.Application.JobApplicationStepFiles;
using Recruiter.Application.Interview.Interfaces;
using Recruiter.Application.Interview;
using Recruiter.Application.JobApplication.Queries;
using Recruiter.Application.JobApplicationStepFiles.Queries;
using Recruiter.Application.Interview.Queries;
using Recruiter.Application.Country;
using Recruiter.Application.Country.Interfaces;
using Recruiter.Application.Comment.Interfaces;
using Recruiter.Application.Comment;
using Recruiter.Application.Comment.Queries;
using Recruiter.Application.Dashboard;
using Recruiter.Application.Dashboard.Interfaces;
using Recruiter.Application.Dashboard.Queries;
using Recruiter.Application.ElevenLabs;
using Recruiter.Application.ElevenLabs.Interfaces;
using Recruiter.Infrastructure.Services;
using Recruiter.Application.Common.Options;
using Recruiter.Application.QuestionnaireTemplate;
using Recruiter.Application.QuestionnaireTemplate.Handlers;
using Recruiter.Application.QuestionnaireTemplate.Import;
using Recruiter.Application.QuestionnaireTemplate.Import.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Queries;
using Recruiter.Application.Questionnaire;
using Recruiter.Application.Questionnaire.Interfaces;


namespace Recruiter.WebApi.Infrastructure;

// Service configuration for dependency injection
public static class ServiceExtension
{
    // Configure application services with proper lifetimes
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
       // Register repositories using helper methods - cleaner and more maintainable
        
        // For BaseDbModel entities (Guid PK)
        services.AddRepository<UserProfile>();
        services.AddRepository<Candidate>();
        services.AddRepository<Domain.Models.File>();
        services.AddRepository<Interview>();
        services.AddRepository<JobApplication>();
        services.AddRepository<JobApplicationStep>();
        services.AddRepository<JobApplicationStepFiles>();
        services.AddRepository<Score>();
        services.AddRepository<Feedback>();
        services.AddRepository<JobPostStepAssignment>();
        services.AddRepository<KanbanBoardColumn>();
        services.AddRepository<Domain.Models.Education>();
        services.AddRepository<Domain.Models.Skill>();
        services.AddRepository<Domain.Models.Experience>();
        services.AddRepository<AwardAchievement>();
        services.AddRepository<CertificationLicense>();
        services.AddRepository<CvEvaluation>();
        services.AddRepository<KeyStrength>();
        services.AddRepository<ProjectResearch>();
        services.AddRepository<Scoring>();
        services.AddRepository<Summary>();
        services.AddRepository<VolunteerExtracurricular>();
        services.AddRepository<Comment>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddBasicRepository<CountryExposureSet>();
        services.AddBasicRepository<CountryExposureSetCountry>();
        services.AddRepository<QuestionnaireSection>();
        services.AddVersionedRepository<QuestionnaireQuestion>();
        services.AddVersionedRepository<QuestionnaireQuestionOption>();
        services.AddRepository<QuestionnaireCandidateSubmissionAnswer>();
        services.AddRepository<QuestionnaireCandidateSubmissionAnswerOption>();
        services.AddRepository<QuestionnaireCandidateSubmission>();

        // Specialized repository to gracefully handle concurrent submission saves
        services.AddScoped<IRepository<QuestionnaireCandidateSubmission>, QuestionnaireCandidateSubmissionRepository>();
        services.AddRepository<QuestionnaireCandidateSubmissionAnswer>();
        services.AddRepository<QuestionnaireCandidateSubmissionAnswerOption>();
        
        // For VersionedBaseDbModel entities (Name+Version PK) - using helper methods
        services.AddVersionedRepository<JobPost>();
        services.AddVersionedRepository<JobPostStep>();
        services.AddVersionedRepository<InterviewConfiguration>();
        services.AddVersionedRepository<Prompt>();
        services.AddVersionedRepository<QuestionnaireTemplate>();
        services.AddVersionedRepository<QuestionnaireQuestion>();
        services.AddVersionedRepository<QuestionnaireQuestionOption>();

        // Specialized repository to gracefully handle frequent concurrent saves from template builder UI
        services.AddScoped<IRepository<QuestionnaireTemplate>, QuestionnaireTemplateRepository>();
        
        // For composite key entities (custom repository in Ardalis)
        services.AddScoped<IJobPostStepAssignmentRepository, JobPostStepAssignmentRepository>();
        services.AddScoped<IInterviewConfigurationRepository, InterviewConfigurationRepository>();
        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        
        // Note: All repositories now use generic IRepository<T> for cleaner code
        // Specific repository interfaces are only used for composite key entities

        // Register application services
        
        // Register JobPost services
        services.AddScoped<ICountryExposureSetService, CountryExposureSetService>();
        services.AddScoped<IJobPostService, JobPostService>();
        services.AddScoped<IJobPostStepService, JobPostStepService>();
        services.AddScoped<IJobPostStepAssignmentService, JobPostStepAssignmentService>();
        services.AddScoped<IJobPostCandidateService, JobPostCandidateService>();
        services.AddScoped<IKanbanBoardColumnService, KanbanBoardColumnService>();
        
        // Register JobApplication services
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<IJobApplicationStepService, JobApplicationStepService>();
        services.AddScoped<IJobApplicationStepFilesService, JobApplicationStepFilesService>();
        services.AddScoped<IJobApplicationCandidateFlowService, JobApplicationCandidateFlowService>();
        
        // Register Interview services
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<ITranscriptPathResolver, Recruiter.Infrastructure.Services.TranscriptPathResolver>();
      
        // Education services
        services.AddScoped<IEducationService, EducationService>();
        
        // Skill services
        services.AddScoped<ISkillService, SkillService>();
        
        // Experience services
        services.AddScoped<IExperienceService, ExperienceService>();
        
        // AwardAchievement services
        services.AddScoped<IAwardAchievementService, AwardAchievementService>();
        
        // CertificationLicense services
        services.AddScoped<ICertificationLicenseService, CertificationLicenseService>();
        
        // KeyStrength services
        services.AddScoped<IKeyStrengthService, KeyStrengthService>();
        
        // VolunteerExtracurricular services
        services.AddScoped<IVolunteerExtracurricularService, VolunteerExtracurricularService>();
        
        // Summary services
        services.AddScoped<ISummaryService, SummaryService>();
        
        // CvEvaluation services
        services.AddScoped<ICvEvaluationService, CvEvaluationService>();
        
        // ProjectResearch services
        services.AddScoped<IProjectResearchService, ProjectResearchService>();

        // InterviewConfiguration services
        services.AddScoped<IInterviewConfigurationService, InterviewConfigurationService>();
        
        // Prompt services
        services.AddScoped<IPromptService, PromptService>();

        // QuestionnaireTemplate services
        services.AddScoped<IQuestionnaireVersioningService, QuestionnaireVersioningService>();
        services.AddScoped<IQuestionnaireTemplateRetryHandler, QuestionnaireTemplateRetryHandler>();
        services.AddScoped<IOptionNameNormalizer, OptionNameNormalizer>();
        services.AddScoped<IQuestionnaireEntityFactory, QuestionnaireEntityFactory>();
        services.AddScoped<OptionSyncHandler>();
        services.AddScoped<QuestionSyncHandler>();
        services.AddScoped<SectionSyncHandler>();
        services.AddScoped<IQuestionnaireTemplateOrchestrator, QuestionnaireTemplateOrchestrator>();
        services.AddScoped<IQuestionnaireTemplateService, QuestionnaireTemplateService>();
        services.AddScoped<IQuestionnaireTemplateImportService, QuestionnaireTemplateImportService>();

        // Candidate questionnaire (assessment step) services
        services.AddScoped<Recruiter.Application.Questionnaire.Queries.QuestionnaireQueryHandler>();
        services.AddScoped<ICandidateQuestionnaireSubmissionOrchestrator, CandidateQuestionnaireSubmissionOrchestrator>();
        services.AddScoped<ICandidateQuestionnaireService, CandidateQuestionnaireService>();
        
        
        // UserProfile services
        services.AddScoped<IUserProfileService, UserProfileService>();
        // Note: ICvProcessingService is registered via AddHttpClient in Program.cs
        
        // File services
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileOrchestrator, FileOrchestrator>();
        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        // Candidate services
        services.AddScoped<ICandidateService, CandidateService>();

        // Comment services
        services.AddScoped<ICommentService, CommentService>();
        
        // Dashboard services
        services.AddScoped<IDashboardService, DashboardService>();

        // Country list (dropdown) service
        services.AddScoped<ICountryListService, CountryListService>();
        
        // ElevenLabs services
        services.AddScoped<IElevenLabsConversationPayloadBuilder, ElevenLabsConversationPayloadBuilder>();
        services.AddScoped<IElevenLabsWebhookValidator, ElevenLabsWebhookValidator>();

        // Sync queue service for sending sync messages to Service Bus
        services.AddSingleton<ISyncQueueService, SyncQueueService>();

        // User registration services
        services.AddUserRegistration();

        services.AddScoped<IJobPostVersioningService, JobPostVersioningService>();

        // Register query handlers
        services.AddScoped<JobPostQueryHandler>();
        services.AddScoped<InterviewConfigurationQueryHandler>();
        services.AddScoped<PromptQueryHandler>();
        services.AddScoped<QuestionnaireTemplateQueryHandler>();
        services.AddScoped<DashboardQueryHandler>();
        services.AddScoped<UserProfileQueryHandler>();
        services.AddScoped<FileQueryHandler>();
        services.AddScoped<CandidateQueryHandler>();
        services.AddScoped<CommentQueryHandler>();
        services.AddScoped<JobApplicationQueryHandler>();
        services.AddScoped<JobApplicationStepFilesQueryHandler>();
        services.AddScoped<InterviewQueryHandler>();

        // Register orchestrators
        services.AddScoped<IJobPostOrchestrator, JobPostOrchestrator>();
        services.AddScoped<IInterviewConfigurationOrchestrator, InterviewConfigurationOrchestrator>();
        services.AddScoped<IPromptOrchestrator, PromptOrchestrator>();
        services.AddScoped<IUserProfileOrchestrator, UserProfileOrchestrator>();
        services.AddScoped<ICandidateOrchestrator, CandidateOrchestrator>();
        services.AddScoped<IFileOrchestrator, FileOrchestrator>();
        
        // Register JobApplicationStepFiles services
        services.AddScoped<Recruiter.Application.JobApplicationStepFiles.Services.IJobApplicationStepResolver, 
            Recruiter.Application.JobApplicationStepFiles.Services.JobApplicationStepResolver>();
        services.AddScoped<Recruiter.Application.JobApplicationStepFiles.Strategies.IFileUploadProcessingStrategy, 
            Recruiter.Application.JobApplicationStepFiles.Strategies.ResumeUploadProcessingStrategy>();
        services.AddScoped<Recruiter.Application.JobApplicationStepFiles.Strategies.IFileUploadProcessingStrategy, 
            Recruiter.Application.JobApplicationStepFiles.Strategies.GenericFileUploadProcessingStrategy>();
        services.AddScoped<JobApplicationStepFileUploadOrchestrator>();
        return services;
    }

    /// Extension method to register repository for BaseDbModel entities (Guid PK)
    public static IServiceCollection AddRepository<TEntity>(this IServiceCollection services)
        where TEntity : BaseDbModel
    {
        // Register generic repository for specific entity type
        services.AddScoped<IRepository<TEntity>, EfRepository<TEntity>>();
        return services;
    }

    /// Extension method to register repository for VersionedBaseDbModel entities (Name+Version PK)
    public static IServiceCollection AddVersionedRepository<TEntity>(this IServiceCollection services)
        where TEntity : VersionedBaseDbModel
    {
        // Register generic repository for specific entity type
        services.AddScoped<IRepository<TEntity>, EfVersionedRepository<TEntity>>();
        return services;
    }

    /// Extension method to register repository for BasicBaseDbModel entities (e.g. CountryExposureSet)
    public static IServiceCollection AddBasicRepository<TEntity>(this IServiceCollection services)
        where TEntity : BasicBaseDbModel
    {
        services.AddScoped<IRepository<TEntity>, EfBasicRepository<TEntity>>();
        return services;
    }

    // Configure AutoMapper profiles
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        // Register AutoMapper profiles from Application assembly
        services.AddAutoMapper(typeof(JobPostProfile).Assembly);

        return services;
    }

    // Configure FluentValidation
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        
        // Register FluentValidation validators from Application assembly
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.JobPost.Validation.JobPostStepDtoValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.JobPost.Validation.JobPostDtoValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.JobPost.Validation.JobPostStepAssignmentDtoValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.InterviewConfiguration.Validation.InterviewConfigurationDtoValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.Prompt.Validation.PromptDtoValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.UserProfile.Validation.UserProfileDtoValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.JobApplicationStepFiles.Validation.GetUploadUrlRequestDtoValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(Recruiter.Application.JobApplicationStepFiles.Validation.CompleteUploadRequestDtoValidator).Assembly);

        // Add FluentValidation ASP.NET Core integration for automatic validation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        return services;
    }

    // Configure logging services
    public static IServiceCollection AddEnhancedLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }

    // Configure performance optimization services
    public static IServiceCollection AddPerformanceServices(this IServiceCollection services)
    {
        // Add memory caching for frequently accessed data
        services.AddMemoryCache();

        // Add response compression
        services.AddResponseCompression();

        // Add response caching
        services.AddResponseCaching();

        return services;
    }
    // Configure Python API authentication services
    public static IServiceCollection AddPythonApiAuthentication(this IServiceCollection services)
    {
        // Register token acquisition service
        services.AddScoped<ITokenAcquisitionService, TokenAcquisitionService>();

        // Register authentication handler (must be transient for HttpClientFactory)
        services.AddTransient<PythonApiAuthenticationHandler>();

        return services;
    }
}
