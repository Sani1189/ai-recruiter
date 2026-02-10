using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate;

/// <summary>
/// Factory for creating questionnaire entities from DTOs.
/// </summary>
public sealed class QuestionnaireEntityFactory : IQuestionnaireEntityFactory
{
    private readonly IOptionNameNormalizer _nameNormalizer;

    public QuestionnaireEntityFactory(IOptionNameNormalizer nameNormalizer)
    {
        _nameNormalizer = nameNormalizer ?? throw new ArgumentNullException(nameof(nameNormalizer));
    }

    public DomainQuestionnaireTemplate CreateTemplate(QuestionnaireTemplateDto dto, int version)
    {
        return new DomainQuestionnaireTemplate
        {
            Name = dto.Name,
            Version = version,
            TemplateType = Enum.TryParse<Domain.Enums.QuestionnaireTemplateTypeEnum>(dto.TemplateType, true, out var templateType)
                ? templateType : Domain.Enums.QuestionnaireTemplateTypeEnum.Form,
            Status = "Draft",
            Title = dto.Title,
            Description = dto.Description,
            TimeLimitSeconds = dto.TimeLimitSeconds,
            PublishedAt = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Sections = new List<QuestionnaireSection>()
        };
    }

    public DomainQuestionnaireTemplate CreateTemplateFromExisting(DomainQuestionnaireTemplate existing, int version)
    {
        return new DomainQuestionnaireTemplate
        {
            Name = existing.Name,
            Version = version,
            TemplateType = existing.TemplateType,
            Status = "Draft",
            Title = existing.Title,
            Description = existing.Description,
            TimeLimitSeconds = existing.TimeLimitSeconds,
            PublishedAt = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Sections = new List<QuestionnaireSection>()
        };
    }

    public QuestionnaireQuestion CreateQuestion(QuestionnaireQuestionDto dto, Guid sectionId)
    {
        return new QuestionnaireQuestion
        {
            Name = dto.Name,
            Version = 1,
            QuestionnaireSectionId = sectionId,
            Order = dto.Order,
            QuestionType = Enum.TryParse<Domain.Enums.QuestionnaireQuestionTypeEnum>(dto.QuestionType, true, out var qt)
                ? qt : Domain.Enums.QuestionnaireQuestionTypeEnum.Text,
            QuestionText = dto.PromptText?.Trim() ?? string.Empty,
            IsRequired = dto.IsRequired,
            TraitKey = dto.TraitKey,
            Ws = dto.Ws,
            MediaUrl = dto.MediaUrl,
            MediaFileId = dto.MediaFileId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Options = new List<QuestionnaireQuestionOption>()
        };
    }

    public QuestionnaireQuestionOption CreateOption(QuestionnaireOptionDto dto, QuestionnaireQuestion question)
    {
        var optionName = _nameNormalizer.NormalizeOptionName(dto, question);

        return new QuestionnaireQuestionOption
        {
            Name = optionName,
            Version = 1,
            QuestionnaireQuestionName = question.Name,
            QuestionnaireQuestionVersion = question.Version,
            Order = dto.Order,
            Label = dto.Label?.Trim() ?? string.Empty,
            MediaUrl = dto.MediaUrl,
            MediaFileId = dto.MediaFileId,
            IsCorrect = dto.IsCorrect,
            Score = dto.Score,
            Weight = dto.Weight,
            Wa = dto.Wa,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
