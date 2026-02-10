using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate.Interfaces;

/// <summary>
/// Factory for creating questionnaire entities from DTOs.
/// </summary>
public interface IQuestionnaireEntityFactory
{
    DomainQuestionnaireTemplate CreateTemplate(QuestionnaireTemplateDto dto, int version);
    DomainQuestionnaireTemplate CreateTemplateFromExisting(DomainQuestionnaireTemplate existing, int version);
    QuestionnaireQuestion CreateQuestion(QuestionnaireQuestionDto dto, Guid sectionId);
    QuestionnaireQuestionOption CreateOption(QuestionnaireOptionDto dto, QuestionnaireQuestion question);
}
