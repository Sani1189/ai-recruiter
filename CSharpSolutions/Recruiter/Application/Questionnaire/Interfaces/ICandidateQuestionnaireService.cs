using Ardalis.Result;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.Questionnaire.Dto;

namespace Recruiter.Application.Questionnaire.Interfaces;

public interface ICandidateQuestionnaireService
{
    Task<Result<CandidateQuestionnaireTemplateDto>> GetTemplateForMyStepAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default);

    Task<Result<CandidateQuestionnaireSubmitResponseDto>> SubmitForMyStepAsync(
        Guid jobApplicationStepId,
        CandidateQuestionnaireSubmitRequestDto request,
        CancellationToken cancellationToken = default);
}

