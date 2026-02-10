using Ardalis.Result;
using Recruiter.Application.Questionnaire.Dto;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.Questionnaire.Interfaces;

/// <summary>
/// Orchestrates complex questionnaire submission operations including validation, scoring, and persistence.
/// </summary>
public interface ICandidateQuestionnaireSubmissionOrchestrator
{
    /// <summary>
    /// Validates and processes a questionnaire submission request.
    /// </summary>
    Task<Result<CandidateQuestionnaireSubmitResponseDto>> ProcessSubmissionAsync(
        Guid jobApplicationStepId,
        Guid candidateId,
        CandidateQuestionnaireSubmitRequestDto request,
        DomainQuestionnaireTemplate template,
        Recruiter.Domain.Models.QuestionnaireCandidateSubmission? existingSubmission,
        CancellationToken cancellationToken = default);
}
