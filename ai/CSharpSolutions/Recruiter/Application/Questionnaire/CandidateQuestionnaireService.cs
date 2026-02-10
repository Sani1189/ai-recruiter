using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Questionnaire.Dto;
using Recruiter.Application.Questionnaire.Interfaces;
using Recruiter.Application.Questionnaire.Queries;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.Questionnaire;

/// <summary>
/// Service for candidate questionnaire operations. Delegates complex logic to orchestrator.
/// </summary>
public sealed class CandidateQuestionnaireService : ICandidateQuestionnaireService
{
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly QuestionnaireQueryHandler _queryHandler;
    private readonly ICandidateQuestionnaireSubmissionOrchestrator _orchestrator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileService _userProfileService;
    private readonly ICandidateService _candidateService;
    private readonly ILogger<CandidateQuestionnaireService> _logger;

    private const string EntityTypeSubmission = "QuestionnaireSubmission";

    public CandidateQuestionnaireService(
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        QuestionnaireQueryHandler queryHandler,
        ICandidateQuestionnaireSubmissionOrchestrator orchestrator,
        ICurrentUserService currentUserService,
        IUserProfileService userProfileService,
        ICandidateService candidateService,
        ILogger<CandidateQuestionnaireService> logger)
    {
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
        _candidateService = candidateService ?? throw new ArgumentNullException(nameof(candidateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<CandidateQuestionnaireTemplateDto>> GetTemplateForMyStepAsync(
        Guid jobApplicationStepId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
            {
                return Result<CandidateQuestionnaireTemplateDto>.Invalid(
                    new ValidationError { ErrorMessage = "Job application step ID is required." });
            }

            var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
            if (candidateId == null)
            {
                LogWarning($"GetTemplateForMyStepAsync: Unauthorized access attempt to questionnaire template: StepId={jobApplicationStepId}");
                return Result<CandidateQuestionnaireTemplateDto>.Unauthorized();
            }

            var contextResult = await _queryHandler.GetQuestionnaireContextAsync(
                jobApplicationStepId, candidateId.Value, cancellationToken);
            if (!contextResult.IsSuccess)
                return TranslateResult<CandidateQuestionnaireTemplateDto>(contextResult);

            var domainTemplateResult = await _queryHandler.GetTemplateWithQuestionsAsync(
                contextResult.Value.TemplateName,
                contextResult.Value.TemplateVersion,
                cancellationToken);
            if (!domainTemplateResult.IsSuccess)
                return TranslateResult<CandidateQuestionnaireTemplateDto>(domainTemplateResult);

            var templateResult = await _queryHandler.GetTemplateForStepAsync(
                contextResult.Value.TemplateName,
                contextResult.Value.TemplateVersion,
                cancellationToken);
            if (!templateResult.IsSuccess)
                return templateResult;

            await EnsureDraftSubmissionAsync(
                jobApplicationStepId,
                contextResult.Value.TemplateName,
                contextResult.Value.TemplateVersion,
                domainTemplateResult.Value.TemplateType,
                cancellationToken);

            return templateResult;
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"GetTemplateForMyStepAsync: Argument null error getting template: StepId={jobApplicationStepId}");
            return Result<CandidateQuestionnaireTemplateDto>.Error("Invalid request parameters.");
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForMyStepAsync: Error getting template for step: StepId={jobApplicationStepId}");
            return Result<CandidateQuestionnaireTemplateDto>.Error("Failed to retrieve questionnaire template.");
        }
    }


    public async Task<Result<CandidateQuestionnaireSubmitResponseDto>> SubmitForMyStepAsync(
        Guid jobApplicationStepId,
        CandidateQuestionnaireSubmitRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
            {
                return Result<CandidateQuestionnaireSubmitResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = "Job application step ID is required." });
            }

            if (request == null)
            {
                return Result<CandidateQuestionnaireSubmitResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = "Request is required." });
            }

            if (request.Answers == null || request.Answers.Count == 0)
            {
                return Result<CandidateQuestionnaireSubmitResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = "At least one answer is required." });
            }

            var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
            if (candidateId == null)
            {
                LogWarning($"SubmitForMyStepAsync: Unauthorized submission attempt: StepId={jobApplicationStepId}");
                return Result<CandidateQuestionnaireSubmitResponseDto>.Unauthorized();
            }

            var contextResult = await _queryHandler.GetQuestionnaireContextAsync(
                jobApplicationStepId, candidateId.Value, cancellationToken);
            if (!contextResult.IsSuccess)
                return TranslateResult<CandidateQuestionnaireSubmitResponseDto>(contextResult);

            var templateResult = await _queryHandler.GetTemplateWithQuestionsAsync(
                contextResult.Value.TemplateName,
                contextResult.Value.TemplateVersion,
                cancellationToken);
            if (!templateResult.IsSuccess)
                return TranslateResult<CandidateQuestionnaireSubmitResponseDto>(templateResult);

            var existingSubmission = await _queryHandler.GetSubmissionForUpdateByStepIdAsync(
                jobApplicationStepId, cancellationToken);

            return await _orchestrator.ProcessSubmissionAsync(
                jobApplicationStepId,
                candidateId.Value,
                request,
                templateResult.Value,
                existingSubmission,
                cancellationToken);
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"SubmitForMyStepAsync: Argument null error submitting questionnaire: StepId={jobApplicationStepId}");
            return Result<CandidateQuestionnaireSubmitResponseDto>.Error("Invalid request parameters.");
        }
        catch (InvalidOperationException ex)
        {
            LogError(ex, $"SubmitForMyStepAsync: Invalid operation submitting questionnaire: StepId={jobApplicationStepId}");
            return Result<CandidateQuestionnaireSubmitResponseDto>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            LogError(ex, $"SubmitForMyStepAsync: Error submitting questionnaire: StepId={jobApplicationStepId}");
            return Result<CandidateQuestionnaireSubmitResponseDto>.Error("Failed to submit questionnaire.");
        }
    }

    private async Task EnsureDraftSubmissionAsync(
        Guid jobApplicationStepId,
        string templateName,
        int templateVersion,
        QuestionnaireTemplateTypeEnum templateType,
        CancellationToken cancellationToken)
    {
        try
        {
            var submission = await _queryHandler.GetSubmissionStatusByStepIdAsync(
                jobApplicationStepId, cancellationToken);

            if (submission == null)
            {
                await CreateNewDraftSubmissionAsync(
                    jobApplicationStepId, templateName, templateVersion, templateType, cancellationToken);
            }
            else if (submission.StartedAt == null)
            {
                await UpdateSubmissionStartTimeAsync(submission, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"EnsureDraftSubmissionAsync: Error ensuring draft submission: StepId={jobApplicationStepId}, TemplateName={templateName}, TemplateVersion={templateVersion}");
            throw new InvalidOperationException($"Failed to ensure draft submission for step {jobApplicationStepId}.", ex);
        }
    }

    private async Task CreateNewDraftSubmissionAsync(
        Guid jobApplicationStepId,
        string templateName,
        int templateVersion,
        QuestionnaireTemplateTypeEnum templateType,
        CancellationToken cancellationToken)
    {
        var submission = new QuestionnaireCandidateSubmission
        {
            Id = Guid.NewGuid(),
            JobApplicationStepId = jobApplicationStepId,
            QuestionnaireTemplateName = templateName,
            QuestionnaireTemplateVersion = templateVersion,
            TemplateType = templateType,
            Status = QuestionnaireSubmissionStatusEnum.Draft,
            StartedAt = DateTimeOffset.UtcNow,
            LastSavedAt = DateTimeOffset.UtcNow
        };

        await _submissionRepository.AddAsync(submission, cancellationToken);
        await _submissionRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateSubmissionStartTimeAsync(
        QuestionnaireCandidateSubmission submission,
        CancellationToken cancellationToken)
    {
        submission.StartedAt = DateTimeOffset.UtcNow;
        await _submissionRepository.UpdateAsync(submission, cancellationToken);
        await _submissionRepository.SaveChangesAsync(cancellationToken);
    }

    private static Result<T> TranslateResult<T>(Result<QuestionnaireQueryHandler.QuestionnaireContext> source) =>
        source.Status switch
        {
            ResultStatus.Unauthorized => Result<T>.Unauthorized(),
            ResultStatus.NotFound => Result<T>.NotFound(),
            ResultStatus.Invalid => Result<T>.Invalid(source.ValidationErrors.ToArray()),
            _ => Result<T>.Error()
        };

    private static Result<T> TranslateResult<T>(Result<DomainQuestionnaireTemplate> source) =>
        source.Status switch
        {
            ResultStatus.NotFound => Result<T>.NotFound(),
            ResultStatus.Unauthorized => Result<T>.Unauthorized(),
            ResultStatus.Invalid => Result<T>.Invalid(source.ValidationErrors.ToArray()),
            _ => Result<T>.Error()
        };

    private async Task<Guid?> GetCurrentCandidateIdAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                LogDebug("GetCurrentCandidateIdAsync: No user email found in current user service");
                return null;
            }

            var userProfile = await _userProfileService.GetByEmailAsync(userEmail, cancellationToken);
            if (!userProfile.IsSuccess || userProfile.Value?.Id == null)
            {
                LogDebug($"GetCurrentCandidateIdAsync: User profile not found for email: {userEmail}");
                return null;
            }

            var candidateResult = await _candidateService.GetByUserIdWithUserProfileAsync(
                userProfile.Value.Id.Value, cancellationToken);

            if (!candidateResult.IsSuccess)
            {
                LogDebug($"GetCurrentCandidateIdAsync: Candidate not found for user ID: {userProfile.Value.Id.Value}");
                return null;
            }

            return candidateResult.Value?.Id;
        }
        catch (Exception ex)
        {
            LogError(ex, "GetCurrentCandidateIdAsync: Error getting current candidate ID");
            return null;
        }
    }

    private void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, message);
    }

    private void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }

    private void LogDebug(string message)
    {
        _logger.LogDebug(message);
    }
}
