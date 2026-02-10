using Recruiter.Application.QuestionnaireTemplate.Dto;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;
using DomainQuestionnaireQuestion = Recruiter.Domain.Models.QuestionnaireQuestion;

namespace Recruiter.Application.QuestionnaireTemplate.Interfaces;

public interface IQuestionnaireTemplateOrchestrator
{
    Task<QuestionnaireTemplateDto> VersionTemplateAsync(
        DomainQuestionnaireTemplate existing,
        QuestionnaireTemplateDto dto,
        CancellationToken cancellationToken = default);

    Task<QuestionnaireTemplateDto?> SyncSectionsAsync(
        DomainQuestionnaireTemplate existing,
        List<QuestionnaireSectionDto> incoming,
        CancellationToken cancellationToken = default);

    Task<QuestionnaireTemplateDto> VersionTemplateForQuestionAsync(
        DomainQuestionnaireTemplate template,
        DomainQuestionnaireQuestion editedQuestion,
        QuestionnaireQuestionDto questionDto,
        CancellationToken cancellationToken = default);
}
