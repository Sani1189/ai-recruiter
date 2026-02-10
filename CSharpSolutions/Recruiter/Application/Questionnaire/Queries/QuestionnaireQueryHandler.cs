using Ardalis.Result;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Application.Questionnaire.Specifications;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;
using DomainAssessmentTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.Questionnaire.Queries;

/// <summary>
/// Handles complex questionnaire queries with optimized performance.
/// Separates query logic from business logic for better maintainability.
/// </summary>
public sealed class QuestionnaireQueryHandler
{
    private readonly IRepository<JobApplicationStep> _jobApplicationStepRepository;
    private readonly IRepository<JobPostStep> _jobPostStepRepository;
    private readonly IRepository<DomainAssessmentTemplate> _assessmentTemplateRepository;
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly IQuestionnaireVersioningService _versioningService;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionnaireQueryHandler> _logger;

    public QuestionnaireQueryHandler(
        IRepository<JobApplicationStep> jobApplicationStepRepository,
        IRepository<JobPostStep> jobPostStepRepository,
        IRepository<DomainAssessmentTemplate> assessmentTemplateRepository,
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        IQuestionnaireVersioningService versioningService,
        IMapper mapper,
        ILogger<QuestionnaireQueryHandler> logger)
    {
        _jobApplicationStepRepository = jobApplicationStepRepository ?? throw new ArgumentNullException(nameof(jobApplicationStepRepository));
        _jobPostStepRepository = jobPostStepRepository ?? throw new ArgumentNullException(nameof(jobPostStepRepository));
        _assessmentTemplateRepository = assessmentTemplateRepository ?? throw new ArgumentNullException(nameof(assessmentTemplateRepository));
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves questionnaire context (template name/version) for a job application step.
    /// Validates candidate ownership and resolves template identity.
    /// </summary>
    public async Task<Result<QuestionnaireContext>> GetQuestionnaireContextAsync(
        Guid jobApplicationStepId,
        Guid candidateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
            {
                return Result<QuestionnaireContext>.Invalid(
                    new ValidationError { ErrorMessage = "Job application step ID is required." });
            }

            if (candidateId == Guid.Empty)
            {
                return Result<QuestionnaireContext>.Invalid(
                    new ValidationError { ErrorMessage = "Candidate ID is required." });
            }

            var step = await _jobApplicationStepRepository.FirstOrDefaultAsync(
                new JobApplicationStepWithApplicationByIdSpec(jobApplicationStepId),
                cancellationToken);

            if (step?.JobApplication == null)
                return Result<QuestionnaireContext>.NotFound();

            if (step.JobApplication.CandidateId != candidateId)
            {
                LogWarning($"GetQuestionnaireContextAsync: Unauthorized access to questionnaire context: StepId={jobApplicationStepId}, CandidateId={candidateId}");
                return Result<QuestionnaireContext>.Unauthorized();
            }

            var existingSubmission = await _submissionRepository.FirstOrDefaultAsync(
                new QuestionnaireSubmissionStatusByStepIdSpec(jobApplicationStepId),
                cancellationToken);

            if (existingSubmission != null &&
                !string.IsNullOrWhiteSpace(existingSubmission.QuestionnaireTemplateName) &&
                existingSubmission.QuestionnaireTemplateVersion > 0)
            {
                return Result<QuestionnaireContext>.Success(
                    new QuestionnaireContext(
                        existingSubmission.QuestionnaireTemplateName.Trim(),
                        existingSubmission.QuestionnaireTemplateVersion));
            }

            var jobPostStep = await _jobPostStepRepository.FirstOrDefaultAsync(
                new JobPostStepByNameAndVersionSpec(step.JobPostStepName, step.JobPostStepVersion),
                cancellationToken);

            if (jobPostStep == null)
                return Result<QuestionnaireContext>.NotFound();

            if (!IsAssessmentStep(jobPostStep.StepType))
            {
                return Result<QuestionnaireContext>.Invalid(
                    new ValidationError { ErrorMessage = "This step is not a Questionnaire step." });
            }

            if (string.IsNullOrWhiteSpace(jobPostStep.QuestionnaireTemplateName))
            {
                return Result<QuestionnaireContext>.Invalid(
                    new ValidationError { ErrorMessage = "Questionnaire template is not configured for this step." });
            }

            var (templateName, templateVersion) = await ResolveTemplateIdentityAsync(jobPostStep, cancellationToken);
            if (templateVersion == null)
                return Result<QuestionnaireContext>.NotFound();

            return Result<QuestionnaireContext>.Success(new QuestionnaireContext(templateName, templateVersion.Value));
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetQuestionnaireContextAsync: Error getting questionnaire context: StepId={jobApplicationStepId}, CandidateId={candidateId}");
            return Result<QuestionnaireContext>.Error("Failed to retrieve questionnaire context.");
        }
    }

    /// <summary>
    /// Gets candidate-safe template (no answer keys/scoring data) for display.
    /// Resolves questions/options to latest versions automatically.
    /// </summary>
    public async Task<Result<CandidateQuestionnaireTemplateDto>> GetTemplateForStepAsync(
        string templateName,
        int templateVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return Result<CandidateQuestionnaireTemplateDto>.Invalid(
                    new ValidationError { ErrorMessage = "Template name is required." });
            }

            if (templateVersion <= 0)
            {
                return Result<CandidateQuestionnaireTemplateDto>.Invalid(
                    new ValidationError { ErrorMessage = "Template version must be greater than zero." });
            }

            var template = await _assessmentTemplateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionForRuntimeSpec(templateName, templateVersion),
                cancellationToken);

            if (template == null)
                return Result<CandidateQuestionnaireTemplateDto>.NotFound();

            var dto = _mapper.Map<CandidateQuestionnaireTemplateDto>(template);
            DeduplicateActiveQuestionsByName(dto);
            return Result<CandidateQuestionnaireTemplateDto>.Success(dto);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForStepAsync: Error getting template for step: TemplateName={templateName}, TemplateVersion={templateVersion}");
            return Result<CandidateQuestionnaireTemplateDto>.Error("Failed to retrieve template.");
        }
    }

    private static void DeduplicateActiveQuestionsByName(CandidateQuestionnaireTemplateDto template)
    {
        // Defensive hardening: if historical data ends up with multiple active versions for the same question name
        // (e.g., order drift + activation logic bug), ensure candidates see only one question per stable name.
        foreach (var section in template.Sections ?? [])
        {
            section.Questions = (section.Questions ?? [])
                .GroupBy(q => (q.Name ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(x => x.Version).First())
                .OrderBy(q => q.Order)
                .ToList();
        }
    }

    /// <summary>
    /// Gets full template with all questions and options (including scoring data) for submission processing.
    /// </summary>
    public async Task<Result<DomainAssessmentTemplate>> GetTemplateWithQuestionsAsync(
        string templateName,
        int templateVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return Result<DomainAssessmentTemplate>.Invalid(
                    new ValidationError { ErrorMessage = "Template name is required." });
            }

            if (templateVersion <= 0)
            {
                return Result<DomainAssessmentTemplate>.Invalid(
                    new ValidationError { ErrorMessage = "Template version must be greater than zero." });
            }

            var template = await _assessmentTemplateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionForSubmissionSpec(templateName, templateVersion),
                cancellationToken);

            if (template == null)
                return Result<DomainAssessmentTemplate>.NotFound();

            return Result<DomainAssessmentTemplate>.Success(template);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateWithQuestionsAsync: Error getting template with questions: TemplateName={templateName}, TemplateVersion={templateVersion}");
            return Result<DomainAssessmentTemplate>.Error("Failed to retrieve template with questions.");
        }
    }

    /// <summary>
    /// Gets submission by step ID with full answers graph (for processing).
    /// </summary>
    public async Task<QuestionnaireCandidateSubmission?> GetSubmissionByStepIdAsync(
        Guid jobApplicationStepId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
                return null;

            return await _submissionRepository.FirstOrDefaultAsync(
                new QuestionnaireSubmissionByStepIdSpec(jobApplicationStepId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetSubmissionByStepIdAsync: Error getting submission by step ID: StepId={jobApplicationStepId}");
            return null;
        }
    }

    /// <summary>
    /// Gets submission row for update (no includes). This is the hot-path for submit;
    /// we only need RowVersion and basic fields, not the full answers graph.
    /// </summary>
    public async Task<QuestionnaireCandidateSubmission?> GetSubmissionForUpdateByStepIdAsync(
        Guid jobApplicationStepId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
                return null;

            return await _submissionRepository.FirstOrDefaultAsync(
                new QuestionnaireSubmissionForUpdateByStepIdSpec(jobApplicationStepId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetSubmissionForUpdateByStepIdAsync: Error getting submission for update: StepId={jobApplicationStepId}");
            return null;
        }
    }

    /// <summary>
    /// Gets submission status only (optimized, no includes).
    /// </summary>
    public async Task<QuestionnaireCandidateSubmission?> GetSubmissionStatusByStepIdAsync(
        Guid jobApplicationStepId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
                return null;

            return await _submissionRepository.FirstOrDefaultAsync(
                new QuestionnaireSubmissionStatusByStepIdSpec(jobApplicationStepId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetSubmissionStatusByStepIdAsync: Error getting submission status: StepId={jobApplicationStepId}");
            return null;
        }
    }

    /// <summary>
    /// Gets job application step with application (for ownership validation).
    /// </summary>
    public async Task<JobApplicationStep?> GetStepWithApplicationAsync(
        Guid jobApplicationStepId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
                return null;

            return await _jobApplicationStepRepository.FirstOrDefaultAsync(
                new JobApplicationStepWithApplicationByIdSpec(jobApplicationStepId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetStepWithApplicationAsync: Error getting step with application: StepId={jobApplicationStepId}");
            return null;
        }
    }

    private async Task<(string Name, int? Version)> ResolveTemplateIdentityAsync(
        JobPostStep jobPostStep,
        CancellationToken cancellationToken)
    {
        var name = jobPostStep.QuestionnaireTemplateName!.Trim();
        var v = jobPostStep.QuestionnaireTemplateVersion;

        if (v.HasValue && v.Value > 0)
        {
            return (name, v.Value);
        }

        var latest = await _assessmentTemplateRepository.FirstOrDefaultAsync(
            new QuestionnaireTemplateLatestByNameSpec(name),
            cancellationToken);

        return (name, latest?.Version);
    }

    private static bool IsAssessmentStep(string? stepType)
    {
        var normalized = (stepType ?? "").Trim();
        return string.Equals(normalized, QuestionnaireConstants.StepTypes.Questionnaire, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(normalized, QuestionnaireConstants.StepTypes.AssessmentLegacy, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(normalized, QuestionnaireConstants.StepTypes.MultipleChoiceLegacy, StringComparison.OrdinalIgnoreCase);
    }

    public sealed record QuestionnaireContext(string TemplateName, int TemplateVersion);

    private void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, message);
    }

    private void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }
}
