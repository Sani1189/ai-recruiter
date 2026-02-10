using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Questionnaire.Dto;
using Recruiter.Application.Questionnaire.Interfaces;
using Recruiter.Application.Questionnaire.Queries;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.Questionnaire;

/// <summary>
/// Orchestrates questionnaire submission operations including validation, scoring, and persistence.
/// </summary>
public sealed class CandidateQuestionnaireSubmissionOrchestrator : ICandidateQuestionnaireSubmissionOrchestrator
{
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly IRepository<QuestionnaireCandidateSubmissionAnswer> _answerRepository;
    private readonly IRepository<JobApplicationStep> _jobApplicationStepRepository;
    private readonly QuestionnaireQueryHandler _queryHandler;
    private readonly ILogger<CandidateQuestionnaireSubmissionOrchestrator> _logger;

    public CandidateQuestionnaireSubmissionOrchestrator(
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        IRepository<QuestionnaireCandidateSubmissionAnswer> answerRepository,
        IRepository<JobApplicationStep> jobApplicationStepRepository,
        QuestionnaireQueryHandler queryHandler,
        ILogger<CandidateQuestionnaireSubmissionOrchestrator> logger)
    {
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _answerRepository = answerRepository ?? throw new ArgumentNullException(nameof(answerRepository));
        _jobApplicationStepRepository = jobApplicationStepRepository ?? throw new ArgumentNullException(nameof(jobApplicationStepRepository));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<CandidateQuestionnaireSubmitResponseDto>> ProcessSubmissionAsync(
        Guid jobApplicationStepId,
        Guid candidateId,
        CandidateQuestionnaireSubmitRequestDto request,
        DomainQuestionnaireTemplate template,
        QuestionnaireCandidateSubmission? existingSubmission,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobApplicationStepId == Guid.Empty)
            {
                return Result<CandidateQuestionnaireSubmitResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = "Job application step ID is required." });
            }

            if (candidateId == Guid.Empty)
            {
                return Result<CandidateQuestionnaireSubmitResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = "Candidate ID is required." });
            }

            if (template == null)
            {
                return Result<CandidateQuestionnaireSubmitResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = "Template is required." });
            }

            var questionById = BuildQuestionDictionary(template);
            var now = DateTimeOffset.UtcNow;

            var validationResult = ValidateSubmissionRequest(request, questionById, existingSubmission, template, now);
            if (!validationResult.IsSuccess)
                return validationResult;

            var buildResult = QuestionnaireAnswerBuilder.BuildAnswers(
                request.Answers,
                questionById,
                template.TemplateType,
                now);

            var submission = existingSubmission ?? await CreateSubmissionAsync(
                jobApplicationStepId,
                template,
                cancellationToken);

            await AddAnswersAsync(submission.Id, buildResult.Answers, cancellationToken);

            var personalityResultJson = PersonalityScoreCalculator.Calculate(
                buildResult.Answers,
                questionById,
                template.TemplateType);

            UpdateSubmission(submission, buildResult, personalityResultJson, now);

            await _submissionRepository.SaveChangesAsync(cancellationToken);

            await MarkStepCompletedAsync(jobApplicationStepId, now, cancellationToken);

            return Result<CandidateQuestionnaireSubmitResponseDto>.Success(new CandidateQuestionnaireSubmitResponseDto
            {
                QuestionnaireSubmissionId = submission.Id,
                Status = submission.Status.ToString(),
                SubmittedAt = submission.SubmittedAt
            });
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"ProcessSubmissionAsync: Argument null error processing submission: StepId={jobApplicationStepId}, CandidateId={candidateId}");
            return Result<CandidateQuestionnaireSubmitResponseDto>.Error("Invalid request parameters.");
        }
        catch (InvalidOperationException ex)
        {
            LogError(ex, $"ProcessSubmissionAsync: Invalid operation processing submission: StepId={jobApplicationStepId}, CandidateId={candidateId}");
            return Result<CandidateQuestionnaireSubmitResponseDto>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            LogError(ex, $"ProcessSubmissionAsync: Error processing submission: StepId={jobApplicationStepId}, CandidateId={candidateId}");
            return Result<CandidateQuestionnaireSubmitResponseDto>.Error("Failed to process questionnaire submission.");
        }
    }

    private Result<CandidateQuestionnaireSubmitResponseDto> ValidateSubmissionRequest(
        CandidateQuestionnaireSubmitRequestDto request,
        Dictionary<(string Name, int Version), QuestionnaireQuestion> questionById,
        QuestionnaireCandidateSubmission? existingSubmission,
        DomainQuestionnaireTemplate template,
        DateTimeOffset now)
    {
        var validationErrors = QuestionnaireSubmissionValidator.ValidateRequest(request, questionById);
        if (validationErrors.Count > 0)
            return Result<CandidateQuestionnaireSubmitResponseDto>.Invalid(validationErrors.ToArray());

        var statusValidation = QuestionnaireSubmissionValidator.ValidateSubmissionStatus<CandidateQuestionnaireSubmitResponseDto>(
            existingSubmission, template, now);
        if (!statusValidation.IsSuccess)
            return statusValidation;

        return Result<CandidateQuestionnaireSubmitResponseDto>.Success(default!);
    }

    private static Dictionary<(string Name, int Version), QuestionnaireQuestion> BuildQuestionDictionary(DomainQuestionnaireTemplate template)
    {
        return template.Sections
            .SelectMany(s => s.Questions ?? [])
            .Where(q => !q.IsDeleted)
            .ToDictionary(q => (q.Name, q.Version), q => q);
    }

    private async Task<QuestionnaireCandidateSubmission> CreateSubmissionAsync(
        Guid jobApplicationStepId,
        DomainQuestionnaireTemplate template,
        CancellationToken cancellationToken)
    {
        var submission = new QuestionnaireCandidateSubmission
        {
            Id = Guid.NewGuid(),
            JobApplicationStepId = jobApplicationStepId,
            QuestionnaireTemplateName = template.Name,
            QuestionnaireTemplateVersion = template.Version,
            TemplateType = template.TemplateType, // From template assigned to job step
            Status = QuestionnaireSubmissionStatusEnum.Draft,
            StartedAt = DateTimeOffset.UtcNow,
            LastSavedAt = DateTimeOffset.UtcNow
        };

        await _submissionRepository.AddAsync(submission, cancellationToken);
        return submission;
    }

    private async Task AddAnswersAsync(
        Guid submissionId,
        List<QuestionnaireCandidateSubmissionAnswer> answers,
        CancellationToken cancellationToken)
    {
        if (answers == null || answers.Count == 0)
            return;

        try
        {
            foreach (var answer in answers)
            {
                answer.QuestionnaireCandidateSubmissionId = submissionId;
                await _answerRepository.AddAsync(answer, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"AddAnswersAsync: Error adding answers: SubmissionId={submissionId}, AnswerCount={answers.Count}");
            throw new InvalidOperationException($"Failed to add answers for submission {submissionId}.", ex);
        }
    }

    private static void UpdateSubmission(
        QuestionnaireCandidateSubmission submission,
        QuestionnaireAnswerBuilder.AnswerBuildResult buildResult,
        string? personalityResultJson,
        DateTimeOffset now)
    {
        submission.LastSavedAt = now;
        submission.SubmittedAt = now;

        var isQuiz = submission.TemplateType == QuestionnaireTemplateTypeEnum.Quiz;
        submission.Status = (isQuiz || buildResult.HasScoredQuestions)
            ? QuestionnaireSubmissionStatusEnum.AutoScored
            : QuestionnaireSubmissionStatusEnum.Submitted;

        if (buildResult.HasScoredQuestions)
        {
            submission.TotalScore = buildResult.TotalScore;
            submission.MaxScore = buildResult.MaxScore;
        }

        if (!string.IsNullOrEmpty(personalityResultJson))
            submission.PersonalityResultJson = personalityResultJson;
    }

    private async Task MarkStepCompletedAsync(
        Guid jobApplicationStepId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        try
        {
            var step = await _queryHandler.GetStepWithApplicationAsync(jobApplicationStepId, cancellationToken);
            if (step == null)
            {
                LogWarning($"MarkStepCompletedAsync: Step not found for completion: StepId={jobApplicationStepId}");
                return;
            }

            step.Status = JobApplicationStepStatusEnum.Completed;
            step.CompletedAt = now;
            await _jobApplicationStepRepository.UpdateAsync(step, cancellationToken);
            await _jobApplicationStepRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"MarkStepCompletedAsync: Error marking step as completed: StepId={jobApplicationStepId}");
            throw new InvalidOperationException($"Failed to mark step {jobApplicationStepId} as completed.", ex);
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
}
